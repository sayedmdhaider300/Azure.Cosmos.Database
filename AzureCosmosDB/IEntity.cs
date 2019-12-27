using Newtonsoft.Json;
using System;

namespace Azure.CosmosDB
{
    public interface IEntity<TKey>
    {
        TKey id { get; set; }
        string _etag { get; set; }
        DateTime createdon { get; set; }
        string createdby { get; set; }
        DateTime modifiedon { get; set; }
        string modifiedby { get; set; }
        [JsonIgnore]
        string partitionkeyvalue { get; }
    }
}
