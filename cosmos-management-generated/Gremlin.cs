using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    class Gremlin
    {
        public async Task<GremlinDatabaseGetResults> CreateDatabaseAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName,
            int? throughput = null,
            bool? autoScale = false)
        {

            GremlinDatabaseCreateUpdateParameters gremlinDatabaseCreateUpdateParameters = new GremlinDatabaseCreateUpdateParameters
            {
                Resource = new GremlinDatabaseResource
                {
                    Id = databaseName
                }
            };

            if (throughput != null)
            {
                //Create database with shared throughput
                gremlinDatabaseCreateUpdateParameters.Options = Throughput.Create(Convert.ToInt32(throughput), Convert.ToBoolean(autoScale));
            }

            return await cosmosClient.GremlinResources.CreateUpdateGremlinDatabaseAsync(resourceGroupName, accountName, databaseName, gremlinDatabaseCreateUpdateParameters);
        }

        public async Task<List<string>> ListDatabasesAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName)
        {

            IEnumerable<GremlinDatabaseGetResults> gremlinDatabases = await cosmosClient.GremlinResources.ListGremlinDatabasesAsync(resourceGroupName, accountName);

            List<string> databaseNames = new List<string>();

            foreach (GremlinDatabaseGetResults gremlinDatabase in gremlinDatabases)
            {
                databaseNames.Add(gremlinDatabase.Name);
            }

            return databaseNames;
        }

        public async Task<GremlinDatabaseGetResults> GetDatabaseAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName)
        {
            GremlinDatabaseGetResults gremlinDatabase = await cosmosClient.GremlinResources.GetGremlinDatabaseAsync(resourceGroupName, accountName, databaseName);

            Console.WriteLine($"Azure Resource Id: {gremlinDatabase.Id}");
            Console.WriteLine($"Database Name: {gremlinDatabase.Resource.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.GremlinResources.GetGremlinDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);
            //Output throughput values
            Console.WriteLine("\nDatabase Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            Console.WriteLine("\n\n-----------------------\n\n");

            return gremlinDatabase;
        }

        public async Task<GremlinGraphGetResults> CreateGraphAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string graphName,
            List<string> partitionKey,
            int? throughput = null,
            bool? autoScale = false)
        {
            GremlinGraphCreateUpdateParameters gremlinGraphCreateUpdateParameters = new GremlinGraphCreateUpdateParameters
            {
                Resource = new GremlinGraphResource
                {
                    Id = graphName,
                    DefaultTtl = -1, //-1 = off, 0 = on no default, >0 = ttl in seconds
                    PartitionKey = new ContainerPartitionKey
                    {
                        Kind = "Hash",
                        Paths = partitionKey, //Only one partition key supported for Gremlin graph
                        Version = 2 //version = 1 is legacy pk with 100 byte hash on pk values. Version 2 for 1 MB hash.
                    },
                    IndexingPolicy = new IndexingPolicy
                    {
                        Automatic = true,
                        IndexingMode = IndexingMode.Consistent,
                        IncludedPaths = new List<IncludedPath>
                        {
                            new IncludedPath { Path = "/*"}
                        },
                        ExcludedPaths = new List<ExcludedPath>
                        {
                            new ExcludedPath { Path = "/myPathToNotIndex/*"},
                            new ExcludedPath { Path = "/_etag/?"}
                        }
                    }
                }
            };

            if (throughput != null)
            {
                //Create graph with dedicated throughput
                gremlinGraphCreateUpdateParameters.Options = Throughput.Create(Convert.ToInt32(throughput), Convert.ToBoolean(autoScale));
            }

            return await cosmosClient.GremlinResources.CreateUpdateGremlinGraphAsync(resourceGroupName, accountName, databaseName, graphName, gremlinGraphCreateUpdateParameters);
        }

        public async Task<List<string>> ListGraphsAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName)
                {
                    IEnumerable<GremlinGraphGetResults> gremlinGraphs = await cosmosClient.GremlinResources.ListGremlinGraphsAsync(resourceGroupName, accountName, databaseName);

                    List<string> containerNames = new List<string>();

                    foreach (GremlinGraphGetResults gremlinGraph in gremlinGraphs)
                    {
                        containerNames.Add(gremlinGraph.Name);
                    }

                    return containerNames;
                }

        public async Task<GremlinGraphGetResults> GetGraphAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            string graphName)
        {
            GremlinGraphGetResults gremlinGraph = await cosmosClient.GremlinResources.GetGremlinGraphAsync(resourceGroupName, accountName, databaseName, graphName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {gremlinGraph.Id}");

            GremlinGraphGetPropertiesResource properties = gremlinGraph.Resource;
            Console.WriteLine($"Graph Name: {properties.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.GremlinResources.GetGremlinGraphThroughputAsync(resourceGroupName, accountName, databaseName, graphName);
            //Output throughput values
            Console.WriteLine("\nGraph Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            int? ttl = properties.DefaultTtl.GetValueOrDefault();
            if (ttl == 0)
                Console.WriteLine($"\n\nGraph TTL: Off");
            else if (ttl == -1)
                Console.WriteLine($"\n\nGraph TTL: On (no default)");
            else
                Console.WriteLine($"\n\nGraph TTL: {ttl} seconds");

            ContainerPartitionKey partitionKey = properties.PartitionKey;
            if (partitionKey != null)
            {
                Console.WriteLine("\nPartition Key Properties\n-----------------------");

                Console.WriteLine($"Partition Key Kind: {partitionKey.Kind}"); //Currently only Hash
                Console.WriteLine($"Partition Key Version: {partitionKey.Version.GetValueOrDefault()}"); //version 2 = large partition key support
                foreach (string path in partitionKey.Paths)
                {
                    Console.WriteLine($"Partition Key Path: {path}"); //Currently just one Partition Key per container
                }
            }

            IndexingPolicy indexingPolicy = properties.IndexingPolicy;
            Console.WriteLine("\nIndexing Policy\n-----------------------");
            Console.WriteLine($"Indexing Mode: {indexingPolicy.IndexingMode}");
            Console.WriteLine($"Automatic: {indexingPolicy.Automatic.GetValueOrDefault()}");

            if (indexingPolicy.IncludedPaths != null)
            {
                Console.WriteLine("\tIncluded Paths\n\t-----------------------");
                foreach (IncludedPath path in indexingPolicy.IncludedPaths)
                {
                    Console.WriteLine($"\tPath: {path.Path}");
                }
                Console.WriteLine("\n\t-----------------------");
            }

            if (indexingPolicy.ExcludedPaths != null)
            {
                Console.WriteLine("\tExcluded Paths\n\t-----------------------");
                foreach (ExcludedPath path in indexingPolicy.ExcludedPaths)
                {
                    Console.WriteLine($"\tPath: {path.Path}");
                }
                Console.WriteLine("\n\t-----------------------");
            }
            return gremlinGraph;
        }

        public async Task<GremlinGraphGetResults> UpdateGraphAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string graphName, 
            int? defaultTtl = null, 
            IndexingPolicy? indexingPolicy = null, 
            Dictionary<string, string>? tags = null)
        {
            //Get the graph and clone it's properties before updating (no PATCH support for child resources)
            GremlinGraphGetResults gremlinGraphGet = await cosmosClient.GremlinResources.GetGremlinGraphAsync(resourceGroupName, accountName, databaseName, graphName);

            GremlinGraphCreateUpdateParameters gremlinGraphCreateUpdateParameters = new GremlinGraphCreateUpdateParameters
            {
                Resource = new GremlinGraphResource
                {
                    Id = graphName,
                    PartitionKey = gremlinGraphGet.Resource.PartitionKey,
                    DefaultTtl = gremlinGraphGet.Resource.DefaultTtl,
                    IndexingPolicy = gremlinGraphGet.Resource.IndexingPolicy
                },
                Options = new CreateUpdateOptions(),
                Tags = gremlinGraphGet.Tags
            };

            //PartitionKey cannot be updated
            if (defaultTtl != null)
                gremlinGraphCreateUpdateParameters.Resource.DefaultTtl = Convert.ToInt32(defaultTtl);

            if (indexingPolicy != null)
                gremlinGraphCreateUpdateParameters.Resource.IndexingPolicy = indexingPolicy;

            if (tags != null)
                gremlinGraphCreateUpdateParameters.Tags = tags;

            return await cosmosClient.GremlinResources.CreateUpdateGremlinGraphAsync(resourceGroupName, accountName, databaseName, graphName, gremlinGraphCreateUpdateParameters);
        }
    }
}
