using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;

namespace Azure.CosmosDB
{
    public class CosmosDBRepository<T> where T : IEntity<string>
    {
        public DocumentClient Client;
        protected internal string PartitionKey;
        protected internal string CollectionId;
        protected internal string DatabaseId;

        public CosmosDBRepository(string endpoint, string authKey, string databaseId, CosmosRetryOptions retryOptions = null)
        {
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(authKey) || string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentNullException();
            }

            var serviceEndpoint = new Uri(endpoint);
            var DatabaseUri = UriFactory.CreateDatabaseUri(databaseId);
            if (retryOptions == null)
            {
                //Default policies
                var connectionPolicy = new ConnectionPolicy
                {
                    EnableEndpointDiscovery = false,
                    MaxConnectionLimit = 1000,
                    RequestTimeout = new TimeSpan(0, 2, 0),
                    RetryOptions = new RetryOptions
                    {
                        MaxRetryAttemptsOnThrottledRequests = 15,
                        MaxRetryWaitTimeInSeconds = 60
                    }
                };

                Client = new DocumentClient(serviceEndpoint, authKey, connectionPolicy);
            }
            else
            {
                //User connection policy
                var connectionPolicy = new ConnectionPolicy
                {
                    EnableEndpointDiscovery = false,
                    MaxConnectionLimit = retryOptions.MaxConnectionLimit,
                    RequestTimeout = retryOptions.RequestTimeout,
                    RetryOptions = new RetryOptions
                    {
                        MaxRetryAttemptsOnThrottledRequests = retryOptions.MaxRetryAttemptsOnThrottledRequests,
                        MaxRetryWaitTimeInSeconds = retryOptions.MaxRetryWaitTimeInSeconds
                    }
                };

                Client = new DocumentClient(serviceEndpoint, authKey, connectionPolicy);
            }

            CollectionId = Util.GetCollectionName<T>();
            PartitionKey = Util.GetPartitionKeyName<T>();
            DatabaseId = databaseId;

            Database db = Client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId }).Result;
            var collection = new DocumentCollection { Id = CollectionId };
            collection.PartitionKey.Paths.Add("/" + PartitionKey);
            DocumentCollection coll = Client.CreateDocumentCollectionIfNotExistsAsync(DatabaseUri, collection).Result;

        }

        public async Task DeleteCollectionAsync(string collectionId)
        {
            await Client.DeleteDocumentCollectionAsync(GetDocumentCollectionUri(collectionId));
        }

        #region Methods

        private Uri GetDocumentCollectionUri(string collectionid)
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionid);
        }

        private Uri GetDocumentUri(string documentid, string collectionId)
        {
            return UriFactory.CreateDocumentUri(DatabaseId, collectionId, documentid);
        }

        private JObject ConvertDynamicEntity(T item)
        {
            Type type = typeof(T);
            PropertyInfo[] objectProperties = type.GetProperties();
            JObject data = JObject.Parse(item.ToString());
            foreach (var property in objectProperties)
            {
                if (property.CanWrite && (property.DeclaringType == typeof(T) || property.DeclaringType == typeof(DynamicEntity)))
                {

                    if (property.PropertyType == typeof(System.DateTime))
                    {
                        object propertyData = property.GetValue(item);
                        data[property.Name] = propertyData == null ? null : Convert.ToDateTime(propertyData).ToString("o", CultureInfo.InvariantCulture);
                    }
                    else
                        data[property.Name] = property.GetValue(item)?.ToString();
                }
            }
            return data;
        }

        public async Task<T> CreateItemAsync(T item)
        {
            Document document;
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
            if (typeof(T).BaseType == typeof(DynamicEntity))
            {
                var data = ConvertDynamicEntity(item);
                document = await Client.CreateDocumentAsync(collectionUri, data);
            }
            else
                document = await Client.CreateDocumentAsync(collectionUri, item);

            return (T)(dynamic)document;
        }

        public async Task<T> UpdateItemAsync(string id, T item, bool addDdateLogs = true)
        {
            var requestOptions = GetRequestOptionsWithAccessCondition(item._etag);
            try
            {
                Document document;
                var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
                if (typeof(T).BaseType == typeof(DynamicEntity))
                {
                    var data = ConvertDynamicEntity(item);
                    document = await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), data, requestOptions);
                }
                else
                    document = await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item, requestOptions); ;
                return (T)(dynamic)document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    throw new ConcurrencyException(ex.Message, ex);
                }
                else
                    throw new Exception(ex.Message, ex);
            }
        }

        public async Task<T> UpdateItemAsync(Expression<Func<T, bool>> predicate, T item)
        {
            T document = default(T);
            var dbItems = await GetItemsAsync(predicate);
            var dbItem = dbItems.FirstOrDefault();
            if (dbItem != null)
            {
                item.id = dbItem.id;
                PropertyInfo[] dataProperties = null;
                var flags = BindingFlags.Public | BindingFlags.Instance;
                dataProperties = dbItem.GetType().GetProperties(flags);
                PropertyInfo[] itemProperties = item.GetType().GetProperties(flags);
                foreach (var property in itemProperties)
                {
                    if (property.GetValue(item) != null && property.CanWrite)
                    {
                        var dataProperty = dataProperties.FirstOrDefault(x => x.Name.Equals(property.Name));
                        dataProperty.SetValue(dbItem, property.GetValue(item));
                    }
                }
                await DeleteItemAsync(dbItem.id, dbItem);
                document = await CreateItemAsync(dbItem);
            }

            return (T)(dynamic)document;
        }

        public async Task<T> DeleteItemAsync(string id, T item)
        {
            RequestOptions requestOptions = new RequestOptions()
            {
                PartitionKey = new PartitionKey(item.partitionkeyvalue),
                AccessCondition = new AccessCondition
                {
                    Condition = item._etag,
                    Type = AccessConditionType.IfMatch
                }
            };
            try
            {
                Document document = await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), requestOptions);
                return (T)(dynamic)document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    throw new ConcurrencyException(ex.Message, ex);
                }
                else
                    throw new Exception(ex.Message, ex);
            }
        }

        public async Task<T> GetItemAsync(string id)
        {
            var documents = await GetItemsAsync(x => x.id.Equals(id));
            return documents.FirstOrDefault();
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate)
        {
            var documents = await GetItemsAsync(predicate);
            return documents.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, bool usePaging = false, int maxItemCount = -1)
        {
            IDocumentQuery<T> query;
            if (predicate != null)
            {
                query = Client.CreateDocumentQuery<T>(
                        UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                        new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = true })
                    .Where(predicate)
                    .AsDocumentQuery();
            }
            else
            {
                query = Client.CreateDocumentQuery<T>(
                        UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                        new FeedOptions { MaxItemCount = maxItemCount, EnableCrossPartitionQuery = true })

                    .AsDocumentQuery();
            }

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        private static RequestOptions GetRequestOptionsWithAccessCondition(string eTag)
        {
            return new RequestOptions
            {
                AccessCondition = new AccessCondition
                {
                    Type = AccessConditionType.IfMatch,
                    Condition = eTag
                }
            };
        }
        private async Task<User> CreateUserIfNotExistAsync(string userId)
        {
            try
            {
                return await Client.ReadUserAsync(UriFactory.CreateUserUri(DatabaseId, userId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var user = await Client.CreateUserAsync(UriFactory.CreateDatabaseUri(DatabaseId), new User { Id = userId });
                    return user;
                }
                else throw e;
            }

        }

        private async Task<Permission> CreatePermissionIfNotExistAsync(User user, string permissionId)
        {
            try
            {
                return await Client.ReadPermissionAsync(UriFactory.CreatePermissionUri(DatabaseId, user.Id, permissionId));
            }
            catch (DocumentClientException e)
            {
                Permission p;
                p = new Permission
                {
                    PermissionMode = PermissionMode.All,
                    ResourceLink = GetDocumentCollectionUri(CollectionId).OriginalString,
                    Id = permissionId
                };
                var permission = await Client.CreatePermissionAsync(user.SelfLink, p);
                return permission;
            }

        }

        public async Task<Permission> GetUserPermission(string userId, string permissionId)
        {
            var user = await CreateUserIfNotExistAsync(userId);
            var permission = await CreatePermissionIfNotExistAsync(user, permissionId);
            return permission;
        }

        #endregion
    }
}
