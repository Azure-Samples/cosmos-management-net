using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_fluent
{
    class MongoDB
    {
        public async Task<IMongoCollection> CreateCollectionAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName)
        {
            await azure.CosmosDBAccounts.Define(accountName)
                .WithRegion(Region.USWest2)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDataModelMongoDB()
                .WithSessionConsistency()
                .WithWriteReplication(Region.USWest2)
                .WithReadReplication(Region.USEast2)
                .DefineNewMongoDB(databaseName)
                    .WithThroughput(400) //Use shared database throughput for MongoDB
                    .DefineNewCollection(collectionName)
                        .WithShardKey("myShardKey")
                        .WithIndex(new MongoIndex 
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
                        })
                        .WithIndex(new MongoIndex
                        {
                            Key = new MongoIndexKeys
                            {
                                Keys = new List<string>
                                    {
                                        "_ts"
                                    }
                            },
                            Options = new MongoIndexOptions { ExpireAfterSeconds = 604800 } //TTL of a week
                        })
                    .Attach()
                .Attach()
            .CreateAsync();

            return await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetMongoDB(databaseName).GetCollectionAsync(collectionName);
        }

        public async Task<IMongoCollection> GetCollectionAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName)
        {
            IMongoCollection mongoDBCollection = await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetMongoDB(databaseName).GetCollectionAsync(collectionName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {mongoDBCollection.Id}");
            Console.WriteLine($"Collection Name: {mongoDBCollection.Id}");

            try
            {
                Console.WriteLine("\nCollection Throughput\n-----------------------");
                ThroughputSettingsGetPropertiesResource throughput = await GetCollectionThroughputSettingsAsync(azure, resourceGroupName, accountName, databaseName, collectionName);
            }
            catch { }

            IReadOnlyDictionary<string, string> shardKeys = mongoDBCollection.ShardKey;
            if (shardKeys.Count > 0)
            {
                Console.WriteLine("\nShard Key Properties\n-----------------------");
                foreach (var shardKey in shardKeys)
                {
                    Console.WriteLine($"Shard Key: {shardKey.Key}, Type: {shardKey.Value}");
                }
            }

            Console.WriteLine("\nIndex Properties\n-----------------------");
            foreach (MongoIndex mongoIndex in mongoDBCollection.Indexes)
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

        public async Task<ThroughputSettingsGetPropertiesResource> GetCollectionThroughputSettingsAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName)
        {
            ThroughputSettingsGetPropertiesResource throughput = await azure.CosmosDBAccounts
                .GetByResourceGroup(resourceGroupName, accountName)
                .GetMongoDB(databaseName)
                .GetCollection(collectionName)
                .GetThroughputSettingsAsync();

            Console.WriteLine($"Current throughput: {throughput.Throughput}");
            Console.WriteLine($"Minimum throughput: {throughput.MinimumThroughput}");
            Console.WriteLine($"Throughput update pending: {throughput.OfferReplacePending}");

            AutopilotSettingsResource autopilot = throughput.AutopilotSettings;
            if (autopilot != null)
            {
                Console.WriteLine("Autopilot enabled: True");
                Console.WriteLine($"Max throughput: {autopilot.MaxThroughput}");
                Console.WriteLine($"Increment percentage: {autopilot.AutoUpgradePolicy.ThroughputPolicy.IncrementPercent}");
            }

            return throughput;
        }
        
        public async Task<int> UpdateCollectionThroughputAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string collectionName, 
            int throughput)
        {
            var throughputSettings = await GetCollectionThroughputSettingsAsync(azure, resourceGroupName, accountName, databaseName, collectionName);

            if (throughputSettings.OfferReplacePending == "true")
                Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

            int minThroughput = Convert.ToInt32(throughputSettings.MinimumThroughput);

            //Check if passed throughput is less than minimum allowable
            if (throughput < minThroughput)
            {
                Console.WriteLine($"Throughput value passed: {throughput} is below Minimum allowable throughput {minThroughput}. Setting to minimum throughput.");
                throughput = minThroughput;
            }

            await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
            .UpdateMongoDB(databaseName)
                .UpdateCollection(collectionName)
                    .WithThroughput(throughput)
                    .Parent()
                .Parent()
            .ApplyAsync();

            return throughput;

        }
    }
}
