using Azure.CosmosDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Cosmos.Repo.Models
{
    [CollectionName("data_import_default_details", "jobKey")]
    public class DataImportDefaultDetails : Entity
    {
        public string jobKey { get; set; }
        public string jobVersion { get; set; }
        public string importHeaderId { get; set; }
        public string importNumber { get; set; }
        public override string partitionkeyvalue
        {
            get
            {
                return this.jobKey;
            }
        }
    }
}
