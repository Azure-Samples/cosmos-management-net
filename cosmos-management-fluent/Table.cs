using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_fluent
{
    class Table
    {

        public async Task<ITable> CreateTableAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string tableName)
        {
            await azure.CosmosDBAccounts.Define(accountName)
                .WithRegion(Region.USWest2)
                .WithExistingResourceGroup(resourceGroupName)
                .WithDataModelAzureTable()
                .WithSessionConsistency()
                .WithWriteReplication(Region.USWest2)
                .WithReadReplication(Region.USEast2)
                    .DefineNewTable(tableName)
                    .Attach()
                .CreateAsync();

            return await azure.CosmosDBAccounts.GetById(accountName).GetTableAsync(tableName);
        }

        public async Task<ThroughputSettingsGetPropertiesResource> GetTableThroughputSettingsAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string tableName)
        {
            ThroughputSettingsGetPropertiesResource throughput = await azure.CosmosDBAccounts
                .GetByResourceGroup(resourceGroupName, accountName)
                .GetTable(tableName)
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

        public async Task<int> UpdateTableThroughputAsync(
            IAzure azure, 
            string resourceGroupName, 
            string accountName, 
            string tableName, 
            int throughput)
        {
            var throughputSettings = await GetTableThroughputSettingsAsync(azure, resourceGroupName, accountName, tableName);

            if (throughputSettings.OfferReplacePending == "true")
            {
                Console.WriteLine($"Cannot update throughput while a throughput update is in progress");
                throughput = 0;
            }
            else
            {
                int minThroughput = Convert.ToInt32(throughputSettings.MinimumThroughput);

                //Check if passed throughput is less than minimum allowable
                if (throughput < minThroughput)
                {
                    Console.WriteLine($"Throughput value passed: {throughput} is below Minimum allowable throughput {minThroughput}. Setting to minimum throughput.");
                    throughput = minThroughput;
                }

                await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
                .UpdateTable(tableName)
                    .WithThroughput(throughput)
                    .Parent()
                .ApplyAsync();
            }

            return throughput;
        }
    }
}
