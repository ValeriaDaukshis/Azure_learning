using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CosmosDb_learning
{
    class Program
    {
        static readonly string endpointUrl = "https://cuzton.documents.azure.com:443/";
        static readonly string autorizationKey = "5V30yHvoAiFp9FltO9syNt4latlnKLMbxZ0JB654DnBgNSkmHTPmnb4Rceh82xX1IOPvbvQzgo3Yjp4S3j25uA==";     
        static CosmosClient client;
        static Container container;
        static Database database;

        private static string databaseId = "FilmDb";
        private static string containerId = "FilmContainer";

        private static List<Film> films;

        static async Task Main(string[] args)
        {
            CreateCollection();
            await CreateDb();
            await CreateContainerAsync();

            //await InsertNewItem(films[0]);
            //await InsertNewItem(films[1]);
            //await UpdateItem(films[0]);
            await ReadAllData();
            //await GetById("1");
            Console.ReadKey();
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

        static async Task GetById(string id)
        {
            var queriable = container.GetItemLinqQueryable<Film>();
            var res = queriable
                .Select(a => a)
                .Where(b => b.Id == id);
            var iterator = res.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                foreach (var f in await iterator.ReadNextAsync())
                {
                    Console.WriteLine($"\tRead by id {f.Id}\n {f.Country}\n {f.Name}\n {f.EnderDate}");
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
