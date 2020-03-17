using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDocument
{
    class Program
    {
        static readonly string endpointUrl = "https://cosmosdb-learning.documents.azure.com:443/";
        static readonly string authorizationKey = "ASmg80a2J3wZ14ZL2oSTycMH5vGAv2wDmu4NYs9IPBLs47fA5cr9awIMpVyvb0TDmDLkDLTh1BxkBIPEYZmJow==";
        static DocumentClient client;

        static async Task Main(string[] args)
        {
            using (client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
            {
                client.CreateDatabaseIfNotExistsAsync(new Database { Id = "Cazton" }).Wait();
                //await CreatePartitionedCollection("Cazton", "Test", "/category");
                //await ChangeCollectionPerformance("Cazton", "Test");
                //await ReadCollectionProperties();
                await ListCollectionsInDatabase();
            }
        }

        //creates collection with default indexing
        static async Task<DocumentCollection> CreatePartitionedCollection(
            string databaseId, string collectionId, string partitionKey)
        {
            DocumentCollection collection = new DocumentCollection();
            collection.Id = collectionId;
            collection.PartitionKey.Paths.Add(partitionKey);

            DocumentCollection documentCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId), collection, new RequestOptions { OfferThroughput = 400 });
            return documentCollection;
        }

        //creates collection with consistent indexing
        static async Task<DocumentCollection> CreatePartitionedCollectionWithCustomIndexing(
            string databaseId, string collectionId, string partitionKey)
        {
            DocumentCollection collection = new DocumentCollection();
            collection.Id = collectionId;
            collection.PartitionKey.Paths.Add(partitionKey);
            collection.IndexingPolicy.IndexingMode = IndexingMode.Consistent;

            DocumentCollection documentCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseId), collection,
                new RequestOptions { OfferThroughput = 1000, ConsistencyLevel = ConsistencyLevel.Session });
            return documentCollection;
        }

        static async Task ChangeCollectionPerformance(string databaseId, string collectionId)
        {
            DocumentCollection collection = await client.ReadDocumentCollectionAsync(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));

            Offer offer = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsEnumerable().Single();

            Console.WriteLine($"Offer {offer} and name: {collection.Id}");

            Offer replased = await client.ReplaceOfferAsync(new OfferV2(offer, 400));

            offer = client.CreateOfferQuery().Where(o => o.ResourceLink == collection.SelfLink).AsEnumerable().Single();

            OfferV2 offerV2 = (OfferV2)offer;

            Console.WriteLine(offerV2.Content.OfferThroughput);
            Console.ReadKey();

        }

        static async Task ReadCollectionProperties()
        {
            DocumentCollection collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("Cazton", "Test"));

            Console.WriteLine($"Collection \n {collection}");
            Console.ReadKey();
        }

        static async Task ListCollectionsInDatabase()
        {
            foreach (var collection in await client.ReadDocumentCollectionFeedAsync(UriFactory.CreateDatabaseUri("Cazton")))
            {
                Console.WriteLine(collection);
            }
            Console.ReadKey();
        }

        static async void AddJSonFromFile(string databaseId, string collectionId, string filePath)
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(line);
                    using (MemoryStream stream = new MemoryStream(byteArray))
                    {
                        await CreateCollectionFromJson(databaseId, collectionId, filePath, stream);
                    }
                }
            }
        }

        static async Task<Document> CreateCollectionFromJson(
            string databaseId, string collectionId, string filePath, Stream stream)
        {
            var collection = await client.ReadDocumentCollectionAsync(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));

            Document document = await client.CreateDocumentAsync(
                collection.Resource.SelfLink, Resource.LoadFrom<Document>(stream));
            return document;
        }
    }
}
