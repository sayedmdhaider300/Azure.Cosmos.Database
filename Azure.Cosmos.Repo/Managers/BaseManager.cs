using Azure.CosmosDB;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Cosmos.Repo.Managers
{
    public class BaseManager<T>
        where T : IEntity<string>
    {
        public CosmosDBRepository<T> repository { get; set; }
        public BaseManager(CosmosDBRepository<T> repository)
        {
            this.repository = repository;
        }

        public async Task<T> CreateItemAsync(T item, bool addDateLogs = true)
        {
            if (addDateLogs)
            {
                item.createdon = DateTime.UtcNow;
                item.modifiedon = DateTime.UtcNow;
            }
            return await repository.CreateItemAsync(item);
        }

        public async Task<T> UpdateItemAsync(string id, T item, bool addDateLogs = true)
        {
            if (addDateLogs)
                item.modifiedon = DateTime.UtcNow;
            return await repository.UpdateItemAsync(id, item);
        }

        public async Task<T> UpdateItemAsync(Expression<Func<T, bool>> predicate, T item, bool addDateLogs = true)
        {
            if (addDateLogs)
                item.modifiedon = DateTime.UtcNow;
            return await repository.UpdateItemAsync(predicate, item);
        }

        public async Task<T> DeleteItemAsync(string id, T item)
        {
            return await repository.DeleteItemAsync(id, item);
        }

        public async Task<T> GetItemAsync(string id)
        {
            return await repository.GetItemAsync(id);
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate)
        {
            return await repository.GetItemAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, bool usePaging = false, int maxItemCount = -1)
        {
            return await repository.GetItemsAsync(predicate, usePaging, maxItemCount);
        }

        public async Task<Permission> GetUserPermission(string userId, string permissionId)
        {
            return await repository.GetUserPermission(userId, permissionId);
        }
    }
}
