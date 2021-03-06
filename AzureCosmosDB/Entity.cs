﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Azure.CosmosDB
{
    public abstract class Entity : IEntity<string>
    {
        public virtual string id { get; set; }
        public DateTime createdon { get; set; }
        public string createdby { get; set; }
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; }
        public string _etag { get; set; }
        [JsonIgnore]
        public abstract string partitionkeyvalue { get; }
    }
}
