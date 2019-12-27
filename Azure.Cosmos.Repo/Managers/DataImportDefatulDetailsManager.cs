using Azure.Cosmos.Repo.Models;
using Azure.CosmosDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Cosmos.Repo.Managers
{
    public class DataImportDefatulDetailsManager : BaseManager<DataImportDefaultDetails>
    {
        public DataImportDefatulDetailsManager(CosmosDBRepository<DataImportDefaultDetails> repository) : base(repository)
        {
        }
    }
}
