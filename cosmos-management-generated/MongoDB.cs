using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    class MongoDB
    {
#pragma warning disable CS8632
        public async Task<MongoDBDatabaseGetResults> CreateDatabaseAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName,
            int? throughput = null,
            bool? autoScale = false)
        {

            MongoDBDatabaseCreateUpdateParameters mongoDBDatabaseCreateUpdateParameters = new MongoDBDatabaseCreateUpdateParameters
            {
                Resource = new MongoDBDatabaseResource
                {
                    Id = databaseName
                },
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.MongoDBResources.CreateUpdateMongoDBDatabaseAsync(resourceGroupName, accountName, databaseName, mongoDBDatabaseCreateUpdateParameters);
        }

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

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);
            //Output throughput values
            Console.WriteLine("\nDatabase Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            Console.WriteLine("\n\n-----------------------\n\n");

            return sqlDatabase;
        }

        public async Task<int> UpdateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName,
            int throughput,
            bool? autoScale = false)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.MongoDBResources.UpdateMongoDBDatabaseThroughputAsync(resourceGroupName, accountName, databaseName, throughputUpdate);

                return throughput;
            }
            catch
            {

                Console.WriteLine("Database throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task MigrateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            bool? autoScale = false)
        {
            try
            {
                if (autoScale.Value)
                {
                    ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.MigrateMongoDBDatabaseToAutoscaleAsync(resourceGroupName, accountName, databaseName);
                    Throughput.Print(throughputSettingsGetResults.Resource);
                }
                else
                {
                    ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.MigrateMongoDBDatabaseToManualThroughputAsync(resourceGroupName, accountName, databaseName);
                    Throughput.Print(throughputSettingsGetResults.Resource);
                }
            }
            catch
            {
                Console.WriteLine("Database throughput not set\nPress any key to continue");
                Console.ReadKey();
            }
        }

        public async Task<MongoDBCollectionGetResults> CreateCollectionAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName, 
            int? throughput = null,
            bool? autoScale = false)
        {
            MongoDBCollectionCreateUpdateParameters mongoDBCollectionCreateUpdateParameters = new MongoDBCollectionCreateUpdateParameters
            {
                Resource = new MongoDBCollectionResource
                {
                    Id = collectionName,
                    ShardKey = new Dictionary<string, string>()
                    {
                        { "myShardKey", "Hash" }
                    },
                    Indexes = new List<MongoIndex>
                    {
                        new MongoIndex
                        {
                             Key = new MongoIndexKeys
                             {
                                 Keys = new List<string>
                                 {
                                     "myShardKey",
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
                //If throughput is null, return empty options for shared collection throughput
                //unless database has no throughput, then defaults to 400 RU/s collection
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.MongoDBResources.CreateUpdateMongoDBCollectionAsync(resourceGroupName, accountName, databaseName, collectionName, mongoDBCollectionCreateUpdateParameters);
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

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName);
            //Output throughput values
            Console.WriteLine("\nCollection Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            IDictionary<string, string> shardKeys = properties.ShardKey;
            if (shardKeys.Count > 0)
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

        public async Task<int> UpdateCollectionThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName,
            int throughput,
            bool? autoScale = false)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.GetMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.MongoDBResources.UpdateMongoDBCollectionThroughputAsync(resourceGroupName, accountName, databaseName, collectionName, throughputUpdate);

                return throughput;
            }
            catch
            {
                Console.WriteLine("Collection throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task MigrateCollectionThroughputAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            string collectionName,
            bool? autoScale = false)
        {
            try
            {
                if (autoScale.Value)
                {
                    ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.BeginMigrateMongoDBCollectionToAutoscaleAsync(resourceGroupName, accountName, databaseName, collectionName);
                    Throughput.Print(throughputSettingsGetResults.Resource);
                }
                else
                {
                    ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.MongoDBResources.BeginMigrateMongoDBCollectionToManualThroughputAsync(resourceGroupName, accountName, databaseName, collectionName);
                    Throughput.Print(throughputSettingsGetResults.Resource);
                }
            }
            catch
            {
                Console.WriteLine("Collection throughput not set\nPress any key to continue");
                Console.ReadKey();
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
                Options = new CreateUpdateOptions(),
                Tags = mongoDBCollectionGet.Tags
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
