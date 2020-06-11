using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    class Cassandra
    {

        public async Task<CassandraKeyspaceGetResults> CreateKeyspaceAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName,
            int? throughput = null,
            bool? autoScale = false)
        {

            CassandraKeyspaceCreateUpdateParameters cassandraKeyspaceCreateUpdateParameters = new CassandraKeyspaceCreateUpdateParameters
            {
                Resource = new CassandraKeyspaceResource
                {
                    Id = keyspaceName
                },
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.CassandraResources.CreateUpdateCassandraKeyspaceAsync(resourceGroupName, accountName, keyspaceName, cassandraKeyspaceCreateUpdateParameters);
        }

        public async Task<List<string>> ListKeyspacesAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName)
        {

            IEnumerable<CassandraKeyspaceGetResults> cassandraKeyspaces = await cosmosClient.CassandraResources.ListCassandraKeyspacesAsync(resourceGroupName, accountName);

            List<string> keyspaceNames = new List<string>();

            foreach (CassandraKeyspaceGetResults cassandraKeyspace in cassandraKeyspaces)
            {
                keyspaceNames.Add(cassandraKeyspace.Name);
            }

            return keyspaceNames;
        }

        public async Task<CassandraKeyspaceGetResults> GetKeyspaceAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string keyspaceName)
        {
            CassandraKeyspaceGetResults cassandraKeyspace = await cosmosClient.CassandraResources.GetCassandraKeyspaceAsync(resourceGroupName, accountName, keyspaceName);

            Console.WriteLine($"Azure Resource Id: {cassandraKeyspace.Id}");
            Console.WriteLine($"Keyspace Name: {cassandraKeyspace.Resource.Id}");

            
            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.CassandraResources.GetCassandraKeyspaceThroughputAsync(resourceGroupName, accountName, keyspaceName);

            //Output throughput values
            Console.WriteLine("\nKeyspace Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);
            
            Console.WriteLine("\n\n-----------------------\n\n");

            return cassandraKeyspace;
        }

        public async Task<int> UpdateKeyspaceThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName,
            int throughput,
            bool? autoScale = false)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.CassandraResources.GetCassandraKeyspaceThroughputAsync(resourceGroupName, accountName, keyspaceName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.CassandraResources.UpdateCassandraKeyspaceThroughputAsync(resourceGroupName, accountName, keyspaceName, throughputUpdate);

                return throughput;

            }
            catch
            {
                Console.WriteLine("Keyspace throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<CassandraTableGetResults> CreateTableAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName, 
            int? throughput = null,
            bool? autoScale = false)
        {

            CassandraTableCreateUpdateParameters cassandraTableCreateUpdateParameters = new CassandraTableCreateUpdateParameters
            {
                Resource = new CassandraTableResource
                {
                    Id = tableName,
                    DefaultTtl = 0, //-1 = off, 0 = on no default, >0 = ttl in seconds
                    Schema =  new CassandraSchema
                    {
                        PartitionKeys = new List<CassandraPartitionKey>
                        {
                            new CassandraPartitionKey { Name = "user_id" }
                        },
                        Columns = new List<Column>
                        {
                            new Column { Name = "user_id", Type = "uuid" },
                            new Column { Name = "posted_month", Type = "int" },
                            new Column { Name = "posted_time", Type = "uuid" },
                            new Column { Name = "body", Type = "text" },
                            new Column { Name = "posted_by", Type = "text" }
                        },
                        ClusterKeys = new List<ClusterKey>
                        {
                            new ClusterKey { Name = "posted_month", OrderBy = "asc" }
                        }
                    }
                },
                Options = Throughput.Create(throughput, autoScale)
            };

            return await cosmosClient.CassandraResources.CreateUpdateCassandraTableAsync(resourceGroupName, accountName, keyspaceName, tableName, cassandraTableCreateUpdateParameters);
        }

        public async Task<List<string>> ListTablesAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string keyspaceName)
        {
            IEnumerable<CassandraTableGetResults> cassandraTables = await cosmosClient.CassandraResources.ListCassandraTablesAsync(resourceGroupName, accountName, keyspaceName);

            List<string> tableNames = new List<string>();

            foreach (CassandraTableGetResults cassandraTable in cassandraTables)
            {
                tableNames.Add(cassandraTable.Name);
            }

            return tableNames;
        }

        public async Task<CassandraTableGetResults> GetTableAsync(
            CosmosDBManagementClient cosmosClient,
            string resourceGroupName,
            string accountName,
            string keyspaceName,
            string tableName)
        {
            CassandraTableGetResults cassandraTable = await cosmosClient.CassandraResources.GetCassandraTableAsync(resourceGroupName, accountName, keyspaceName, tableName);

            Console.WriteLine("\n\n-----------------------");
            Console.WriteLine($"Azure Resource Id: {cassandraTable.Id}");

            CassandraTableGetPropertiesResource properties = cassandraTable.Resource;
            Console.WriteLine($"Table Name: {properties.Id}");

            ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.CassandraResources.GetCassandraTableThroughputAsync(resourceGroupName, accountName, keyspaceName, tableName);
            //Output throughput values
            Console.WriteLine("\nTable Throughput\n-----------------------");
            Throughput.Print(throughputSettingsGetResults.Resource);

            int? ttl = properties.DefaultTtl.GetValueOrDefault();
            if (ttl == 0)
                Console.WriteLine($"\n\nTable TTL: Off");
            else if (ttl == -1)
                Console.WriteLine($"\n\nTable TTL: On (no default)");
            else
                Console.WriteLine($"\n\nTable TTL: {ttl} seconds");

            CassandraSchema schema = properties.Schema;

            Console.WriteLine("\nTable Columns\n----------------------");
            foreach (Column column in schema.Columns)
            {
                Console.WriteLine($"Column Name: {column.Name}, Type: {column.Type}");
            }

            Console.WriteLine("\nPartition Key Properties\n-----------------------");
            foreach (CassandraPartitionKey partitionKey in schema.PartitionKeys)
            {
                Console.WriteLine($"Partition Key: {partitionKey.Name}"); //Currently just one Partition Key per container
            }

            Console.WriteLine("\nCluster Key Properties\n-----------------------");
            foreach (ClusterKey clusterKey in schema.ClusterKeys)
            {
                Console.WriteLine($"Cluster Key: {clusterKey.Name}, Order By: {clusterKey.OrderBy}");
            }

            return cassandraTable;
        }

        public async Task<int> UpdateTableThroughputAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName,
            int throughput,
            bool? autoScale = false)
        {

            try
            {
                ThroughputSettingsGetResults throughputSettingsGetResults = await cosmosClient.CassandraResources.GetCassandraTableThroughputAsync(resourceGroupName, accountName, keyspaceName, tableName);

                ThroughputSettingsUpdateParameters throughputUpdate = Throughput.Update(throughputSettingsGetResults.Resource, throughput, autoScale);

                await cosmosClient.CassandraResources.UpdateCassandraTableThroughputAsync(resourceGroupName, accountName, keyspaceName, tableName, throughputUpdate);

                return throughput;
            }
            catch
            {
                Console.WriteLine("Table throughput not set\nPress any key to continue");
                Console.ReadKey();
                return 0;
            }
        }

        public async Task<CassandraTableGetResults> UpdateTableAsync(
            CosmosDBManagementClient cosmosClient, 
            string resourceGroupName, 
            string accountName, 
            string keyspaceName, 
            string tableName, 
            int? defaultTtl = null, 
            List<Column>? cassandraColumns = null, 
            Dictionary<string, string>? tags = null)
        {
            //Get the table and clone it's properties before updating (no PATCH support for child resources)
            CassandraTableGetResults cassandraTableGet = await cosmosClient.CassandraResources.GetCassandraTableAsync(resourceGroupName, accountName, keyspaceName, tableName);

            CassandraTableCreateUpdateParameters cassandraTableCreateUpdateParameters = new CassandraTableCreateUpdateParameters
            {
                Resource = new CassandraTableResource
                {
                    Id = tableName,
                    DefaultTtl = cassandraTableGet.Resource.DefaultTtl,
                    Schema = cassandraTableGet.Resource.Schema
                },
                Options = new CreateUpdateOptions(),
                Tags = cassandraTableGet.Tags
            };

            //PartitionKey and ClusterKey in Schema are immutable. Only DefaultTtl, Columns and Tags can be changed.
            if (defaultTtl != null)
                cassandraTableCreateUpdateParameters.Resource.DefaultTtl = Convert.ToInt32(defaultTtl);

            if (cassandraColumns != null)
                cassandraTableCreateUpdateParameters.Resource.Schema.Columns = cassandraColumns;

            if (tags != null)
                cassandraTableCreateUpdateParameters.Tags = tags;

            return await cosmosClient.CassandraResources.CreateUpdateCassandraTableAsync(
                resourceGroupName, accountName, keyspaceName, tableName, cassandraTableCreateUpdateParameters);
        }
    }
}
