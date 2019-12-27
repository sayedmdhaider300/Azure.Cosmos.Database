using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Azure.CosmosDB
{
    public abstract class EntityNoLogFields : IEntity<string>
    {
        public virtual string id { get; set; }
        [JsonIgnore]
        public DateTime createdon { get; set; }
        [JsonIgnore]
        public string createdby { get; set; }
        [JsonIgnore]
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        public string modifiedby { get; set; }
        [JsonIgnore]
        public abstract string partitionkeyvalue { get; }
        public string _etag { get; set; }
    }
}
