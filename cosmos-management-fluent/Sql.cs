using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_fluent
{
    public class Sql
    {
        public async Task<ISqlDatabase> AddDatabaseToAccountAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            int? throughput = null)
        {
            ICosmosDBAccount account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

            if(throughput != null)
            { 
                await account.Update()
                    .DefineNewSqlDatabase(databaseName)
                        .WithThroughput(throughput.GetValueOrDefault())
                        .Attach()
                    .ApplyAsync();
            }
            else
            {
                await account.Update()
                    .DefineNewSqlDatabase(databaseName)
                        .Attach()
                    .ApplyAsync();
            }

            return await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetSqlDatabaseAsync(databaseName);

        }

        public async Task<ISqlContainer> AddContainerToDatabaseAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            int throughput)
        {
            ICosmosDBAccount account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

            await account.Update()
                .UpdateSqlDatabase(databaseName)
                .DefineNewSqlContainer(containerName)
                    .WithThroughput(throughput)
                    .WithPartitionKey(PartitionKind.Hash, null )
                    .WithPartitionKeyPath("/myPartitionKey")
                    .DefineIndexingPolicy()
                        .WithAutomatic(true)
                        .WithIndexingMode(IndexingMode.Consistent)
                        .Attach()
                    .Attach()
                    .Parent()
                .ApplyAsync();

            return await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetSqlDatabase(databaseName).GetSqlContainerAsync(containerName);
                    
        }
        
        public async Task<ISqlContainer> CreateContainerAllAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName)
        {
            string storedProcedureName = "storedProcedure1";
            string triggerName = "preTriggerAll1";
            TriggerType triggerType = TriggerType.Pre;
            TriggerOperation triggerOperation = TriggerOperation.All;
            string userDefinedFunctionName = "userDefinedFunction1";
            string storedProcedureBody = File.ReadAllText($@".\js\{storedProcedureName}.js");
            string triggerBody = File.ReadAllText($@".\js\{triggerName}.js");
            string userDefinedFunctionBody = File.ReadAllText($@".\js\{userDefinedFunctionName}.js");

            await azure.CosmosDBAccounts.Define(accountName)
                .WithRegion(Region.USWest2)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDataModelSql()
                .WithSessionConsistency()
                .WithWriteReplication(Region.USWest2)
                .WithReadReplication(Region.USEast2)
                .DefineNewSqlDatabase(databaseName)
                    .DefineNewSqlContainer(containerName)
                        .WithThroughput(400)
                        .WithPartitionKey(PartitionKind.Hash, null)
                        .WithPartitionKeyPath("/myPartitionKey")
                        .DefineIndexingPolicy()
                            .WithAutomatic(true)
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPath("/*")
                            .WithExcludedPath("/myPathToNotIndex/*")
                            .WithSpatialIndex("/mySpatialPath/*", SpatialType.Point, SpatialType.LineString, SpatialType.Polygon, SpatialType.MultiPolygon )
                            .WithNewCompositeIndexList()
                                .WithCompositePath("/myOrderByPath1", CompositePathSortOrder.Ascending)
                                .WithCompositePath("/myOrderByPath2", CompositePathSortOrder.Descending)
                                .Attach()
                            .Attach()
                        .WithUniqueKey()
                        .WithUniqueKey("/myUniqueKey1", "/myUniqueKey2")
                        .WithConflictResolutionPath(ConflictResolutionMode.LastWriterWins, "/myConflictResolverPath")
                    .WithStoredProcedure(storedProcedureName, storedProcedureBody )
                    .WithTrigger(triggerName, triggerBody, triggerType, triggerOperation)
                    //still not fluent for 1.32
                    .WithUserDefinedFunction(userDefinedFunctionName, new SqlUserDefinedFunctionResource { Id = userDefinedFunctionName, Body = userDefinedFunctionBody })
                    .Attach()
                .Attach()
            .CreateAsync();

            return await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetSqlDatabase(databaseName).GetSqlContainerAsync(containerName);

        }

        public async Task ListSqlContainersAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName)
        {
            var containers = await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetSqlDatabase(databaseName).ListSqlContainersAsync();
            
            foreach(ISqlContainer container in containers)
            {
                Console.WriteLine($"Container name: {container.Name}");
            }
        }

        public async Task<ISqlContainer> GetContainerAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName)
        {
            ISqlContainer sqlContainer = await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetSqlDatabase(databaseName).GetSqlContainerAsync(containerName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {sqlContainer.Id}");
            Console.WriteLine($"Container Name: {sqlContainer.Name}");

            try
            {
                ThroughputSettingsGetPropertiesResource throughput = await GetContainerThroughputSettingsAsync(azure, resourceGroupName, accountName, databaseName, containerName);

                Console.WriteLine("\nContainer Throughput\n-----------------------");
                Console.WriteLine($"Provisioned Container Throughput: {throughput.Throughput}");
                Console.WriteLine($"Minimum Container Throughput: {throughput.MinimumThroughput}");
                Console.WriteLine($"Offer Replace Pending: {throughput.OfferReplacePending}");
            }
            catch { }

            int? ttl = sqlContainer.DefaultTtl.GetValueOrDefault();
            if (ttl == 0)
                Console.WriteLine($"\n\nContainer TTL: Off");
            else if (ttl == -1)
                Console.WriteLine($"\n\nContainer TTL: On (no default)");
            else
                Console.WriteLine($"\n\nContainer TTL: {ttl} seconds");

            ContainerPartitionKey partitionKey = sqlContainer.PartitionKey;
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

            IndexingPolicy indexingPolicy = sqlContainer.IndexingPolicy;
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
                    foreach (SpatialType type in spec.Types)
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

            if (sqlContainer.UniqueKeyPolicy.UniqueKeys.Count > 0)
            {
                Console.WriteLine("Unique Key Policies\n\t-----------------------");
                int iKey = 1;
                foreach (UniqueKey uniqueKey in sqlContainer.UniqueKeyPolicy.UniqueKeys)
                {
                    Console.WriteLine($"\tUnique Key #:{iKey}");
                    foreach (string path in uniqueKey.Paths)
                    {
                        Console.WriteLine($"\tUnique Key Path: {path}");
                    }
                    Console.WriteLine("\t-----------------------");
                    iKey++;
                    if (sqlContainer.UniqueKeyPolicy.UniqueKeys.Count > iKey)
                        Console.WriteLine("\t-----------------------");
                }
            }

            if (azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).MultipleWriteLocationsEnabled.GetValueOrDefault())
            {   //Use some logic here to distinguish "custom" merge using stored procedure versus just writing to the conflict feed "none".
                if (sqlContainer.ConflictResolutionPolicy.Mode == ConflictResolutionMode.Custom)
                {
                    if (sqlContainer.ConflictResolutionPolicy.ConflictResolutionProcedure.Length == 0)
                    {
                        Console.WriteLine("Conflict Resolution Mode: Asynchronous via Conflict Feed");
                    }
                    else
                    {
                        Console.WriteLine("Conflict Resolution Mode: Custom Merge Procedure");
                        Console.WriteLine($"Conflict Resolution Stored Procedure: {sqlContainer.ConflictResolutionPolicy.ConflictResolutionProcedure}");
                    }
                }
                else
                {   //Last Writer Wins
                    Console.WriteLine($"Conflict Resolution Mode: {sqlContainer.ConflictResolutionPolicy.Mode.Value}");
                    Console.WriteLine($"Conflict Resolution Path: {sqlContainer.ConflictResolutionPolicy.ConflictResolutionPath}");
                }
            }
            Console.WriteLine("\n\n-----------------------\n\n");

            return sqlContainer;
        }

        public async Task<ThroughputSettingsGetPropertiesResource> GetContainerThroughputSettingsAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName)
        {
            ThroughputSettingsGetPropertiesResource throughput = await azure.CosmosDBAccounts
                .GetByResourceGroup(resourceGroupName, accountName)
                .GetSqlDatabase(databaseName)
                .GetSqlContainer(containerName)
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
        
        public async Task<int> UpdateContainerThroughputAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName, 
            int throughput)
        {
            var throughputSettings = await GetContainerThroughputSettingsAsync(azure, resourceGroupName, accountName, databaseName, containerName);


            if (throughputSettings.OfferReplacePending == "true")
            {
                Console.WriteLine($"Cannot update throughput while a throughput update is in progress");
                throughput = 0;
            }
            else
            { 
                int minThroughput = Convert.ToInt32(throughputSettings.MinimumThroughput);

                //Check if passed throughput is less than minimum allowable
                if(throughput < minThroughput)
                {
                    Console.WriteLine($"Throughput value passed: {throughput} is below Minimum allowable throughput {minThroughput}. Setting to minimum throughput.");
                    throughput = minThroughput;
                }

                await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
                .UpdateSqlDatabase(databaseName)
                    .UpdateSqlContainer(containerName)
                        .WithThroughput(throughput)
                        .Parent()
                    .Parent()
                .ApplyAsync();
            }

            return throughput;

        }

        public async Task UpdateContainerAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string databaseName, 
            string containerName)
        {

            await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
                .UpdateSqlDatabase(databaseName)
                    .UpdateSqlContainer(containerName)
                        .UpdateIndexingPolicy()
                            .WithoutExcludedPath("/myPathToNotIndex/*") //add back to index
                            .Parent()
                        .Parent()
                    .Parent()
                .ApplyAsync();
        }
    }
}
