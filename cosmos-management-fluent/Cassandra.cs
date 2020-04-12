using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_fluent
{
    class Cassandra
    {

        public async Task<ICassandraTable> CreateTableAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName)
        {
            await azure.CosmosDBAccounts.Define(accountName)
                .WithRegion(Region.USWest2)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDataModelCassandra()
                .WithEventualConsistency()
                .WithWriteReplication(Region.USWest2)
                .WithReadReplication(Region.USEast2)
                .DefineNewCassandraKeyspace(keyspaceName)
                    .DefineNewCassandraTable(tableName)
                        .WithThroughput(400)
                        .WithPartitionKey("user_id")
                        .WithColumn("user_id", "int")
                        .WithColumn("posted_month", "int")
                        .WithColumn("name", "text")
                        .WithColumn("zipcode", "text")
                        .WithColumn("description", "text")
                        .WithClusterKey(new ClusterKey { Name = "user_id", OrderBy="asc" })
                        .WithClusterKey(new ClusterKey { Name = "posted_month", OrderBy = "asc" })
                        .Attach()
                    .Attach()
                .CreateAsync();

            return await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).GetCassandraKeyspace(keyspaceName).GetCassandraTableAsync(tableName);
        }

        public async Task<ThroughputSettingsGetPropertiesResource> GetTableThroughputSettingsAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName)
        {
            ThroughputSettingsGetPropertiesResource throughput = await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).
                GetCassandraKeyspace(keyspaceName).
                GetCassandraTable(tableName)
                .GetThroughputSettingsAsync();

            Console.WriteLine($"Current throughput: {throughput.Throughput}");
            Console.WriteLine($"Minimum throughput: {throughput.MinimumThroughput}");
            Console.WriteLine($"Throughput update pending: {throughput.OfferReplacePending}");

            AutopilotSettingsResource autopilot = throughput.AutopilotSettings;
            if(autopilot != null)
            {
                Console.WriteLine("Autopilot enabled: True");
                Console.WriteLine($"Max throughput: {autopilot.MaxThroughput}");
                Console.WriteLine($"Increment percentage: {autopilot.AutoUpgradePolicy.ThroughputPolicy.IncrementPercent}");
            }

            return throughput;
        }

        public async Task<int> UpdateTableThroughputAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName, 
            int throughput)
        {
            var throughputSettings = await GetTableThroughputSettingsAsync(azure, resourceGroupName, accountName, keyspaceName, tableName);

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
                .UpdateCassandraKeyspace(keyspaceName)
                    .UpdateCassandraTable(tableName)
                        .WithThroughput(throughput)
                    .Parent()
                .Parent()
            .ApplyAsync();
                
            return throughput;
        }

        public async Task UpdateTableAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName)
        {

            await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
                .UpdateCassandraKeyspace(keyspaceName)
                    .UpdateCassandraTable(tableName)
                        .WithColumn("posted_year", "int")
                        .Parent()
                    .Parent()
                .ApplyAsync();

        }
    }
}
