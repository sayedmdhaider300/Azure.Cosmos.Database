using Azure.Cosmos.Repo.Managers;
using Azure.Cosmos.Repo.Models;
using Azure.CosmosDB;
using MainMethod.CosmosDB;
using System;

namespace MainMethod
{
    class Program
    {
  
        static void Main(string[] args)
        {
            try
            {
                var data = CreateRandomImportDefaultDetails();
                CosmosDBProgram cosmosDb = new CosmosDBProgram();
                cosmosDb.CreateData(data).Wait();
            }
            catch (Exception ex)
            {

            }
        }
        public static DataImportDefaultDetails CreateRandomImportDefaultDetails()
        {
            var randomNumber = new Random();
            var dataImportDefaultDetails = new DataImportDefaultDetails()
            {
                jobKey = (Guid.NewGuid()).ToString(),
                jobVersion = randomNumber.Next(1, 100).ToString(),
                importHeaderId = (Guid.NewGuid()).ToString(),
                importNumber = "IMP " + randomNumber.Next(100, 999).ToString()
            };
            return dataImportDefaultDetails;
        }
    }
}
