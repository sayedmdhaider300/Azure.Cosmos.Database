using Autofac;
using Azure.Cosmos.Repo.Managers;
using Azure.Cosmos.Repo.Models;
using Azure.CosmosDB;
using CT.KeyVault;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MainMethod.CosmosDB
{
    public class CosmosDBProgram
    {
        private static DataImportDefatulDetailsManager dataImportDefatulDetailsManager;
        private const string endpoint = "https://localhost:8081/";
        private const string databaseId = "localDatabase";
        private const string authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        public CosmosDBProgram()
        {
            var cosmosDBRepository = new CosmosDBRepository<DataImportDefaultDetails>(endpoint, authKey, databaseId);
            dataImportDefatulDetailsManager = new DataImportDefatulDetailsManager(cosmosDBRepository);
        }
        public async Task CreateData(DataImportDefaultDetails data)
        {
            await dataImportDefatulDetailsManager.CreateItemAsync(data);
        }
    }
}
