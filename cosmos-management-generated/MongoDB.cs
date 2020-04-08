using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    class MongoDB
    {

        public async Task<List<string>> ListDatabasesAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName)
        {

            IEnumerable<MongoDBDatabaseGetResults> mongoDBDatabases = await cosmosClient.MongoDBResources.ListMongoDBDatabasesAsync(resourceGroupName, accountName);

            List<string> databaseNames = new List<string>();

            foreach (MongoDBDatabaseGetResults mongoDBDatabase in mongoDBDatabases)
            {
                databaseNames.Add(mongoDBDatabase.Name);
            }

            return databaseNames;
        }

        public async Task<MongoDBDatabaseGetResults> GetDatabaseAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName)
        {
            MongoDBDatabaseGetResults sqlDatabase = await cosmosClient.MongoDBResources.GetMongoDBDatabaseAsync(resourceGroupName, accountName, databaseName);

            Console.WriteLine($"Azure Resource Id: {sqlDatabase.Id}");
            Console.WriteLine($"Database Name: {sqlDatabase.Resource.Id}");

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);

                ThroughputSettingsGetPropertiesResource throughput = throughputSettingsGetResults.Resource;

                Console.WriteLine("\nDatabase Throughput\n-----------------------");
                Console.WriteLine($"Provisioned Database Throughput: {throughput.Throughput}");
                Console.WriteLine($"Minimum Database Throughput: {throughput.MinimumThroughput}");
                Console.WriteLine($"Offer Replace Pending: {throughput.OfferReplacePending}");

            }
            catch { }

            Console.WriteLine("\n\n-----------------------\n\n");

            return sqlDatabase;
        }

        public async Task<MongoDBDatabaseGetResults> CreateDatabaseAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            int throughput)
        {

            MongoDBDatabaseCreateUpdateParameters mongoDBDatabaseCreateUpdateParameters = new MongoDBDatabaseCreateUpdateParameters
            {
                Resource = new MongoDBDatabaseResource
                {
                    Id = databaseName
                },
                Options = new Dictionary<string, string>()
                {
                    { "Throughput", throughput.ToString() }
                }
            };

            return await cosmosClient.MongoDBResources.CreateUpdateMongoDBDatabaseAsync(resourceGroupName, accountName, databaseName, mongoDBDatabaseCreateUpdateParameters);
        }

        public async Task<int> UpdateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            int throughput)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);

                ThroughputSettingsGetPropertiesResource throughputResource = throughputSettingsGetResults.Resource;

                if (throughputResource.OfferReplacePending == "true")
                    Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

                int minThroughput = Convert.ToInt32(throughputResource.MinimumThroughput);

                //Never set below min throughput or will generate exception
                if (minThroughput > throughput)
                    throughput = minThroughput;

                await cosmosClient.MongoDBResources.UpdateMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName, new
                    ThroughputSettingsUpdateParameters(new ThroughputSettingsResource(throughput)));

                return throughput;
            }
            catch
            {

                Console.WriteLine("Database throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<List<string>> ListCollectionsAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName)
        {
            IEnumerable<MongoDBCollectionGetResults> mongoDBCollections = await cosmosClient.MongoDBResources.ListMongoDBCollectionsAsync(resourceGroupName, accountName, databaseName);

            List<string> containerNames = new List<string>();

            foreach (MongoDBCollectionGetResults mongoDBCollection in mongoDBCollections)
            {
                containerNames.Add(mongoDBCollection.Name);
            }

            return containerNames;
        }

        public async Task<MongoDBCollectionGetResults> GetCollectionAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName)
        {
            MongoDBCollectionGetResults mongoDBCollection = await cosmosClient.MongoDBResources.GetMongoDBCollectionAsync(resourceGroupName, accountName, databaseName, collectionName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {mongoDBCollection.Id}");

            MongoDBCollectionGetPropertiesResource properties = mongoDBCollection.Resource;
            Console.WriteLine($"Collection Name: {properties.Id}");

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName);

                ThroughputSettingsGetPropertiesResource throughput = throughputSettingsGetResults.Resource;

                Console.WriteLine("\nCollection Throughput\n-----------------------");
                Console.WriteLine($"Provisioned Collection Throughput: {throughput.Throughput}");
                Console.WriteLine($"Minimum Collection Throughput: {throughput.MinimumThroughput}");
                Console.WriteLine($"Offer Replace Pending: {throughput.OfferReplacePending}");
            }
            catch { }

            IDictionary<string, string> shardKeys = properties.ShardKey;
            if(shardKeys.Count > 0)
            { 
                Console.WriteLine("\nShard Key Properties\n-----------------------");
                foreach (var shardKey in shardKeys)
                {
                    Console.WriteLine($"Shard Key: {shardKey.Key}, Type: {shardKey.Value}");
                }
            }

            Console.WriteLine("\nIndex Properties\n-----------------------");
            foreach (MongoIndex mongoIndex in properties.Indexes)
            {
                MongoIndexKeys mongoIndexKeys = mongoIndex.Key;
                MongoIndexOptions mongoIndexOptions = mongoIndex.Options;

                foreach (string key in mongoIndexKeys.Keys)
                {
                    Console.WriteLine($"Key: {key}");
                }

                if (mongoIndexOptions.Unique.GetValueOrDefault())
                    Console.WriteLine($"unique: {mongoIndexOptions.Unique}");

                if (mongoIndexOptions.ExpireAfterSeconds.HasValue)
                    Console.WriteLine($"expireAfterSeconds:{mongoIndexOptions.ExpireAfterSeconds}");
            }

            return mongoDBCollection;
        }

        public async Task<MongoDBCollectionGetResults> CreateCollectionAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName, 
            string shardKey, 
            int throughput)
        {
            MongoDBCollectionCreateUpdateParameters mongoDBCollectionCreateUpdateParameters = new MongoDBCollectionCreateUpdateParameters
            {
                Resource = new MongoDBCollectionResource
                {
                    Id = collectionName,
                    ShardKey = new Dictionary<string, string>()
                    {
                        { shardKey, "Hash" }
                    },
                    Indexes = new List<MongoIndex>
                    {
                        new MongoIndex
                        {
                             Key = new MongoIndexKeys
                             {
                                 Keys = new List<string>
                                 {
                                     "user_id",
                                     "user_address"
                                 }
                             },
                             Options = new MongoIndexOptions { Unique = true }
                        },
                        new MongoIndex
                        {
                            Key = new MongoIndexKeys
                            {
                                Keys = new List<string>
                                { 
                                    "_ts" 
                                }
                            },
                            Options = new MongoIndexOptions { ExpireAfterSeconds = 604800 } //TTL of a week
                        }
                    }
                    
                },
                Options = new Dictionary<string, string>()
                {
                    { "Throughput", throughput.ToString() }
                }
            };

            return await cosmosClient.MongoDBResources.CreateUpdateMongoDBCollectionAsync(resourceGroupName, accountName, databaseName, collectionName, mongoDBCollectionCreateUpdateParameters);
        }

        public async Task<int> UpdateCollectionThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName, 
            int throughput)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName);

                ThroughputSettingsGetPropertiesResource throughputResource = throughputSettingsGetResults.Resource;

                if (throughputResource.OfferReplacePending == "true")
                    Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

                int minThroughput = Convert.ToInt32(throughputResource.MinimumThroughput);

                //Never set below min throughput or will generate exception
                if (minThroughput > throughput)
                    throughput = minThroughput;

                await cosmosClient.MongoDBResources.UpdateMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName, new
                ThroughputSettingsUpdateParameters(new ThroughputSettingsResource(throughput)));

                return throughput;
            }
            catch
            {
                Console.WriteLine("Collection throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<MongoDBCollectionGetResults> UpdateCollectionAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName, 
            IList<MongoIndex>? mongoIndexes = null, 
            Dictionary<string, string>? tags = null)
        {
            //Get the collection and clone it's properties before updating (no PATCH support for child resources)
            MongoDBCollectionGetResults mongoDBCollectionGet = await cosmosClient.MongoDBResources.GetMongoDBCollectionAsync(resourceGroupName, accountName, databaseName, collectionName);

            MongoDBCollectionCreateUpdateParameters mongoDBCollectionCreateUpdateParameters = new MongoDBCollectionCreateUpdateParameters
            {
                Resource = new MongoDBCollectionResource
                {
                    Id = collectionName,
                    ShardKey = mongoDBCollectionGet.Resource.ShardKey,
                    Indexes = mongoDBCollectionGet.Resource.Indexes
                },
                Tags = mongoDBCollectionGet.Tags,
                Options = new Dictionary<string, string>() { }
            };

            //ShardKey cannot be updated
            if (mongoIndexes != null)
                mongoDBCollectionCreateUpdateParameters.Resource.Indexes = mongoIndexes;

            if(tags != null)
                mongoDBCollectionCreateUpdateParameters.Tags = tags;

            return await cosmosClient.MongoDBResources.CreateUpdateMongoDBCollectionAsync(resourceGroupName, accountName, databaseName, collectionName, mongoDBCollectionCreateUpdateParameters);
        }

    }
}
