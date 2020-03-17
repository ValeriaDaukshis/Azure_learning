using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;

namespace CosmosDb_learning
{
    class Program
    {
        static readonly string endpointUrl = "https://cosmosdb-learning.documents.azure.com:443/";
        static readonly string autorizationKey = "ASmg80a2J3wZ14ZL2oSTycMH5vGAv2wDmu4NYs9IPBLs47fA5cr9awIMpVyvb0TDmDLkDLTh1BxkBIPEYZmJow==";     
        static CosmosClient client;
        static Microsoft.Azure.Cosmos.Container container;
        static Database database;

        private static string databaseId = "FilmDatabase";
        private static string containerId = "FilmContainer";

        private static List<Film> films;

        static async Task Main(string[] args)
        {
            CreateCollection();
            await CreateDb();
            await CreateContainerAsync();
            //await ReadAllData();
            //await InsertNewItem(films[0]);
            //await UpdateItem(films[0]);
        }

        private static async Task CreateDb()
        {
            client = new CosmosClient(endpointUrl, autorizationKey);
            database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        static async Task CreateContainerAsync()
        {
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/Country");
        }

        static async Task InsertNewItem(Film f)
        {
            var response = await container.CreateItemAsync<Film>(f, new PartitionKey(f.Country));
        }

        static async Task DeleteItem(string id)
        {
            var response = await container.DeleteItemAsync<Film>(id, new PartitionKey("/Country"));
        }

        static async Task ReadAllData()
        {
            var sqlQueryText = "SELECT * FROM films";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Film> queryResultSetIterator = container.GetItemQueryIterator<Film>(queryDefinition);

            List<Film> films = new List<Film>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Film> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Film f in currentResultSet)
                {
                    films.Add(f);
                    Console.WriteLine($"\tRead {f.Country}\n {f.Name}\n {f.EnderDate}");
                }
            }
        }
        

        static async Task UpdateItem(Film f)
        {
            f.Name = $"New {f.Name}";
            var response = await container.ReplaceItemAsync<Film>(f, f.Id, new PartitionKey(f.Country));
        }

        static void CreateCollection()
        {
            films = new List<Film> {
                new Film
                {
                    Id = "1",
                    Name = "Despicable me!",
                    EnderDate = DateTime.Now,
                    Country = "USA"
                },
                new Film
                {
                    Id = "2",
                    Name = "Sonic",
                    EnderDate = DateTime.Now,
                    Country = "USA"
                },
            };
        }

        
    }
}
