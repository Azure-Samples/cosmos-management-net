using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Rest;
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
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {

            GremlinDatabaseCreateUpdateParameters gremlinDatabaseCreateUpdateParameters = new GremlinDatabaseCreateUpdateParameters
            {
                Resource = new GremlinDatabaseResource
                {
                    Id = databaseName
                },
                Options = Throughput.Create(throughput, autoScale, autoUpgrade, incrementPercent)
            };

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
            Throughput.Get(throughputSettingsGetResults.Resource);

            Console.WriteLine("\n\n-----------------------\n\n");

            return gremlinDatabase;
        }

        public async Task<int> UpdateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.GremlinResources.GetGremlinDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale, autoUpgrade, incrementPercent);

                await cosmosClient.GremlinResources.UpdateGremlinDatabaseThroughputAsync(resourceGroupName, accountName, databaseName, throughputUpdate);

                return throughput;
            }
            catch
            {
                Console.WriteLine("Database throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<GremlinGraphGetResults> CreateGraphAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string graphName, 
            string partitionKey,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
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
                        Paths = new List<string> { partitionKey },
                        Version = 1 //version 2 for large partition key
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
                            new ExcludedPath { Path = "/myPathToNotIndex/*"}
                        },
                        CompositeIndexes = new List<IList<CompositePath>>
                        {
                            new List<CompositePath>
                            {
                                new CompositePath { Path = "/myOrderByPath1", Order = CompositePathSortOrder.Ascending },
                                new CompositePath { Path = "/myOrderByPath2", Order = CompositePathSortOrder.Descending }
                            },
                            new List<CompositePath>
                            {
                                new CompositePath { Path = "/myOrderByPath3", Order = CompositePathSortOrder.Ascending },
                                new CompositePath { Path = "/myOrderByPath4", Order = CompositePathSortOrder.Descending }
                            }
                        }
                    }
                },
                Options = Throughput.Create(throughput, autoScale, autoUpgrade, incrementPercent)
            };

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
            Throughput.Get(throughputSettingsGetResults.Resource);

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
            Console.WriteLine($"Automatic: {indexingPolicy.Automatic.Value.ToString()}");

            if (indexingPolicy.IncludedPaths.Count > 0)
            {
                Console.WriteLine("\tIncluded Paths\n\t-----------------------");
                foreach (IncludedPath path in indexingPolicy.IncludedPaths)
                {
                    Console.WriteLine($"\tPath: {path.Path}");
                }
                Console.WriteLine("\n\t-----------------------");
            }

            if (indexingPolicy.ExcludedPaths.Count > 0)
            {
                Console.WriteLine("\tExcluded Paths\n\t-----------------------");
                foreach (ExcludedPath path in indexingPolicy.ExcludedPaths)
                {
                    Console.WriteLine($"\tPath: {path.Path}");
                }
                Console.WriteLine("\n\t-----------------------");
            }

            if (indexingPolicy.SpatialIndexes.Count > 0)
            {
                Console.WriteLine("\tSpatial Indexes\n\t-----------------------");
                foreach (SpatialSpec spec in indexingPolicy.SpatialIndexes)
                {
                    Console.WriteLine($"\tPath: {spec.Path}");
                    Console.WriteLine("\t\tSpatial Types\n\t\t-----------------------");
                    foreach (string type in spec.Types)
                    {
                        Console.WriteLine($"\t\tType: {type}");
                    }
                }
                Console.WriteLine("\n\t-----------------------");
            }

            if (indexingPolicy.CompositeIndexes.Count > 0)
            {
                Console.WriteLine("\tComposite Indexes\n\t-----------------------");

                int iIndex = 1;
                foreach (List<CompositePath> compositePaths in indexingPolicy.CompositeIndexes)
                {
                    Console.WriteLine($"\tComposite Index #:{iIndex}");
                    foreach (CompositePath compositePath in compositePaths)
                    {
                        Console.WriteLine($"\tPath: {compositePath.Path}, Order: {compositePath.Order}");
                    }
                    Console.WriteLine("\t-----------------------");
                    iIndex++;
                    if (compositePaths.Count > iIndex)
                        Console.WriteLine("\t-----------------------");
                }
            }

            if (cosmosClient.DatabaseAccounts.GetAsync(resourceGroupName, accountName).Result.EnableMultipleWriteLocations.GetValueOrDefault())
            {   //Use some logic here to distinguish "custom" merge using stored procedure versus just writing to the conflict feed "none".
                if (properties.ConflictResolutionPolicy.Mode == "Custom")
                {
                    if (properties.ConflictResolutionPolicy.ConflictResolutionProcedure.Length == 0)
                    {
                        Console.WriteLine("Conflict Resolution Mode: Conflict Feed");
                    }
                    else
                    {
                        Console.WriteLine("Conflict Resolution Mode: Custom Merge Procedure");
                        Console.WriteLine($"Conflict Resolution Stored Procedure: {properties.ConflictResolutionPolicy.ConflictResolutionProcedure}");
                    }
                }
                else
                {   //Last Writer Wins
                    Console.WriteLine($"Conflict Resolution Mode: {properties.ConflictResolutionPolicy.Mode}");
                    Console.WriteLine($"Conflict Resolution Path: {properties.ConflictResolutionPolicy.ConflictResolutionPath}");
                }
            }
            Console.WriteLine("\n\n-----------------------\n\n");

            return gremlinGraph;
        }

        public async Task<int> UpdateGraphThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string graphName,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.GremlinResources.GetGremlinGraphThroughputAsync(resourceGroupName, accountName, databaseName, graphName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale, autoUpgrade, incrementPercent);

                await cosmosClient.GremlinResources.UpdateGremlinGraphThroughputAsync(resourceGroupName, accountName, databaseName, graphName, throughputUpdate);

                return throughput;
            }
            catch
            {
                Console.WriteLine("Graph throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<GremlinGraphGetResults> UpdateGraphAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string graphName, 
            int? defaultTtl = null, 
            IndexingPolicy? indexingPolicy = null, 
            Dictionary<string, string> tags = null)
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
