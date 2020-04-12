using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Rest;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;


namespace cosmos_management_generated
{
    class Table
    {

        public async Task<TableGetResults> CreateTableAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string tableName,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {
            TableCreateUpdateParameters tableCreateUpdateParameters = new TableCreateUpdateParameters
            {
                Resource = new TableResource
                {
                    Id = tableName
                },
                //Account-level shared throughput is not supported via Control Plane
                Options = Throughput.Create(throughput, autoScale, autoUpgrade, incrementPercent)
            };

            return await cosmosClient.TableResources.CreateUpdateTableAsync(resourceGroupName, accountName, tableName, tableCreateUpdateParameters);
        }

        public async Task<List<string>> ListTablesAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName)
        {
            IEnumerable<TableGetResults> tables = await cosmosClient.TableResources.ListTablesAsync(resourceGroupName, accountName);

            List<string> tableNames = new List<string>();

            foreach (TableGetResults table in tables)
            {
                tableNames.Add(table.Name);
            }

            return tableNames;
        }

        public async Task<TableGetResults> GetTableAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string tableName)
        {
            TableGetResults table = await cosmosClient.TableResources.GetTableAsync(resourceGroupName, accountName, tableName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {table.Id}");

            TableGetPropertiesResource properties = table.Resource;
            Console.WriteLine($"Table Name: {properties.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.TableResources.GetTableThroughputAsync(resourceGroupName, accountName, tableName);
            //Output throughput values
            Console.WriteLine("\nTable Throughput\n-----------------------");
            Throughput.Get(throughputSettingsGetResults.Resource);

            return table;
        }
        
        public async Task<int> UpdateTableThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string tableName,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.TableResources.GetTableThroughputAsync(resourceGroupName, accountName, tableName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale, autoUpgrade, incrementPercent);

                await cosmosClient.TableResources.UpdateTableThroughputAsync(resourceGroupName, accountName, tableName, throughputUpdate);

                return throughput;
            }
            catch
            {
                Console.WriteLine("Table throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<TableGetResults> UpdateTableAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string tableName, 
            Dictionary<string, string>? tags = null)
        {
            //Get the table and clone it's properties before updating (no PATCH support for child resources)
            TableGetResults tableGet = await cosmosClient.TableResources.GetTableAsync(resourceGroupName, accountName, tableName);

            TableCreateUpdateParameters tableCreateUpdateParameters = new TableCreateUpdateParameters
            {
                Resource = new TableResource
                {
                    Id = tableName,
                },
                Tags = tableGet.Tags 
            };

            //The only thing mutable in Table is Tags
            if (tags != null)
                tableCreateUpdateParameters.Tags = tags;

            return await cosmosClient.TableResources.CreateUpdateTableAsync(
                resourceGroupName, accountName, tableName, tableCreateUpdateParameters);
        }
    }
}
