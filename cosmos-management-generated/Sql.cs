using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    public class Sql
    {

        public async Task<SqlDatabaseGetResults> CreateDatabaseAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            int? throughput = null,
            bool? autoScale = false)
        {

            SqlDatabaseCreateUpdateParameters sqlDatabaseCreateUpdateParameters = new SqlDatabaseCreateUpdateParameters
            {
                Resource = new SqlDatabaseResource
                {
                    Id = databaseName
                },
                //If throughput is null, return empty options, for dedicated container throughput
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.SqlResources.CreateUpdateSqlDatabaseAsync(resourceGroupName, accountName, databaseName, sqlDatabaseCreateUpdateParameters);
        }

        public async Task<List<string>> ListDatabasesAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName)
        {

            IEnumerable<SqlDatabaseGetResults> sqlDatabases = await cosmosClient.SqlResources.ListSqlDatabasesAsync(resourceGroupName, accountName);

            List<string> databaseNames = new List<string>();

            foreach (SqlDatabaseGetResults sqlDatabase in sqlDatabases)
            {
                databaseNames.Add(sqlDatabase.Name);
            }

            return databaseNames;
        }

        public async Task<SqlDatabaseGetResults> GetDatabaseAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName)
        {
            SqlDatabaseGetResults sqlDatabase = await cosmosClient.SqlResources.GetSqlDatabaseAsync(resourceGroupName, accountName, databaseName);

            Console.WriteLine($"Azure Resource Id: {sqlDatabase.Id}");
            Console.WriteLine($"Database Name: {sqlDatabase.Resource.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.SqlResources.GetSqlDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);
            //Output throughput values
            Console.WriteLine("\nDatabase Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            Console.WriteLine("\n\n-----------------------\n\n");

            return sqlDatabase;
        }

        public async Task UpdateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            int throughput, 
            bool? autoScale = false)
        {
            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.SqlResources.GetSqlDatabaseThroughputAsync(resourceGroupName, accountName, databaseName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.SqlResources.UpdateSqlDatabaseThroughputAsync(resourceGroupName, accountName, databaseName, throughputUpdate);
            }
            catch 
            {
                Console.WriteLine("Database throughput not set\nPress any key to continue");
                Console.ReadKey();
            }
        }

        public async Task MigrateDatabaseThroughputAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            int throughput,
            bool? autoScale = false)
            {
                throw new Exception("Not implemented");

            }

        public async Task<SqlContainerGetResults> CreateContainerAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            string partitionKey,
            int? throughput = null,
            bool? autoScale = false)
        {
            SqlContainerCreateUpdateParameters sqlContainerCreateUpdateParameters = new SqlContainerCreateUpdateParameters
            {
                Resource = new SqlContainerResource
                {
                    Id = containerName,
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
                        SpatialIndexes = new List<SpatialSpec>
                        {
                            new SpatialSpec {
                                Path = "/mySpatialPath/*",
                                Types = new List<string> { "Point", "LineString", "Polygon", "MultiPolygon" }
                            }
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
                    },
                    UniqueKeyPolicy = new UniqueKeyPolicy
                    {
                        UniqueKeys = new List<UniqueKey>
                        {
                           new UniqueKey {
                               Paths = new List<string>
                               {
                                   "/myUniqueKey1",
                                   "/myUniqueKey2"
                               }
                            }
                        }
                    },
                    ConflictResolutionPolicy = new ConflictResolutionPolicy //only for multi-master mode
                    {
                        Mode = ConflictResolutionMode.LastWriterWins,
                        ConflictResolutionPath = "/myConflictResolverPath"
                    }
                },
                //If throughput is null, return empty options for shared container throughput
                //unless database has no throughput, then defaults to 400 RU/s container
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.SqlResources.CreateUpdateSqlContainerAsync(resourceGroupName, accountName, databaseName, containerName, sqlContainerCreateUpdateParameters);
        }

        public async Task<List<string>> ListContainersAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName)
        {
            IEnumerable<SqlContainerGetResults> sqlContainers = await cosmosClient.SqlResources.ListSqlContainersAsync(resourceGroupName, accountName, databaseName);

            List<string> containerNames = new List<string>();

            foreach (SqlContainerGetResults sqlContainer in sqlContainers)
            {
                containerNames.Add(sqlContainer.Name);
            }

            return containerNames;
        }
        
        public async Task<SqlContainerGetResults> GetContainerAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            string containerName)
        {
            SqlContainerGetResults sqlContainer = await cosmosClient.SqlResources.GetSqlContainerAsync(resourceGroupName, accountName, databaseName, containerName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {sqlContainer.Id}");

            SqlContainerGetPropertiesResource properties = sqlContainer.Resource;
            Console.WriteLine($"Container Name: {properties.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.SqlResources.GetSqlContainerThroughputAsync(resourceGroupName, accountName, databaseName, containerName);
            //Output throughput values
            Console.WriteLine("\nContainer Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            int? ttl = properties.DefaultTtl.GetValueOrDefault();
            if (ttl == 0)
                Console.WriteLine($"\n\nContainer TTL: Off");
            else if (ttl == -1)
                Console.WriteLine($"\n\nContainer TTL: On (no default)");
            else
                Console.WriteLine($"\n\nContainer TTL: {ttl} seconds");

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

            if (indexingPolicy.SpatialIndexes != null)
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

            if (indexingPolicy.CompositeIndexes != null)
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

            if (properties.UniqueKeyPolicy != null)
            {
                Console.WriteLine("Unique Key Policies\n\t-----------------------");
                int iKey = 1;
                foreach (UniqueKey uniqueKey in properties.UniqueKeyPolicy.UniqueKeys)
                {
                    Console.WriteLine($"\tUnique Key #:{iKey}");
                    foreach (string path in uniqueKey.Paths)
                    {
                        Console.WriteLine($"\tUnique Key Path: {path}");
                    }
                    Console.WriteLine("\t-----------------------");
                    iKey++;
                    if (properties.UniqueKeyPolicy.UniqueKeys.Count > iKey)
                        Console.WriteLine("\t-----------------------");
                }
            }

            if (cosmosClient.DatabaseAccounts.GetAsync(resourceGroupName, accountName).Result.EnableMultipleWriteLocations.GetValueOrDefault())
            {   //Use some logic here to distinguish "custom" merge using stored procedure versus just writing to the conflict feed "none".
                if (properties.ConflictResolutionPolicy.Mode == "Custom")
                {
                    if (properties.ConflictResolutionPolicy.ConflictResolutionProcedure.Length == 0)
                    {
                        Console.WriteLine("Conflict Resolution Mode: Asynchronous via Conflict Feed");
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

            return sqlContainer;
        }

        public async Task UpdateContainerThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            int throughput,
            bool? autoScale = false)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.SqlResources.GetSqlContainerThroughputAsync(resourceGroupName, accountName, databaseName, containerName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.SqlResources.UpdateSqlContainerThroughputAsync(resourceGroupName, accountName, databaseName, containerName, throughputUpdate);
            }
            catch
            {
                Console.WriteLine("Container throughput not set\nPress any key to continue");
                Console.ReadKey();
            }
        }

        public async Task MigrateContainerThroughputAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string databaseName,
            string containerName,
            int throughput,
            bool? autoScale = false)
            {

                throw new Exception("Not implemented");
            
            }

        public async Task<SqlContainerGetResults> UpdateContainerAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            int? defaultTtl = null, 
            IndexingPolicy? indexingPolicy = null, 
            ConflictResolutionPolicy? conflictResolutionPolicy = null, 
            Dictionary<string, string>? tags = null)
        {
            //Get the container and clone it's properties before updating (no PATCH support for child resources)
            SqlContainerGetResults sqlContainerGet = await cosmosClient.SqlResources.GetSqlContainerAsync(resourceGroupName, accountName, databaseName, containerName);

            SqlContainerCreateUpdateParameters sqlContainerCreateUpdateParameters = new SqlContainerCreateUpdateParameters
            {
                Resource = new SqlContainerResource
                {
                    Id = containerName,
                    PartitionKey = sqlContainerGet.Resource.PartitionKey,
                    UniqueKeyPolicy = sqlContainerGet.Resource.UniqueKeyPolicy,
                    DefaultTtl = sqlContainerGet.Resource.DefaultTtl,
                    IndexingPolicy = sqlContainerGet.Resource.IndexingPolicy,
                    ConflictResolutionPolicy = sqlContainerGet.Resource.ConflictResolutionPolicy,
                },
                Options = new CreateUpdateOptions(),
                Tags = sqlContainerGet.Tags
            };

            //PartitionKey and UniqueKeyPolicy cannot be updated
            if (defaultTtl != null)
                sqlContainerCreateUpdateParameters.Resource.DefaultTtl = Convert.ToInt32(defaultTtl);

            if (indexingPolicy != null)
                sqlContainerCreateUpdateParameters.Resource.IndexingPolicy = indexingPolicy;

            if (conflictResolutionPolicy != null)
                sqlContainerCreateUpdateParameters.Resource.ConflictResolutionPolicy = conflictResolutionPolicy;

            if (tags != null)
                sqlContainerCreateUpdateParameters.Tags = tags;

            return await cosmosClient.SqlResources.CreateUpdateSqlContainerAsync(resourceGroupName, accountName, databaseName, containerName, sqlContainerCreateUpdateParameters);
        }

        public async Task<SqlStoredProcedureGetResults> CreateStoredProcedureAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            string storedProcedureName, 
            string body)
        {
            SqlStoredProcedureCreateUpdateParameters storedProcedureCreateUpdateParameters = new SqlStoredProcedureCreateUpdateParameters
            {
                Resource = new SqlStoredProcedureResource
                {
                    Id = storedProcedureName,
                    Body = body
                },
                Options = new CreateUpdateOptions { }
            };

            return await cosmosClient.SqlResources.CreateUpdateSqlStoredProcedureAsync(resourceGroupName, accountName, databaseName, containerName, storedProcedureName, storedProcedureCreateUpdateParameters);
        }

        public async Task<SqlTriggerGetResults> CreateTriggerAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            string triggerName, 
            string triggerOperation, 
            string triggerType, 
            string body)
        {
            SqlTriggerCreateUpdateParameters sqlTriggerCreateUpdateParameters = new SqlTriggerCreateUpdateParameters
            {
                Resource = new SqlTriggerResource
                {
                    Id = triggerName,
                    TriggerOperation = triggerOperation,
                    TriggerType = triggerType,
                    Body = body
                },
                Options = new CreateUpdateOptions { }
            };

            return await cosmosClient.SqlResources.CreateUpdateSqlTriggerAsync(resourceGroupName, accountName, databaseName, containerName, triggerName, sqlTriggerCreateUpdateParameters);
        }
        
        public async Task<SqlUserDefinedFunctionGetResults> CreateUserDefinedFunctionAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            string userDefinedFunctionName, 
            string body)
        {
            SqlUserDefinedFunctionCreateUpdateParameters sqlUserDefinedFunctionCreateUpdateParameters = new SqlUserDefinedFunctionCreateUpdateParameters
            {
                Resource = new SqlUserDefinedFunctionResource
                {
                    Id = userDefinedFunctionName,
                    Body = body
                },
                Options = new CreateUpdateOptions { }
            };

            return await cosmosClient.SqlResources.CreateUpdateSqlUserDefinedFunctionAsync(resourceGroupName, accountName, databaseName, containerName, userDefinedFunctionName, sqlUserDefinedFunctionCreateUpdateParameters);
        }
    }
}
