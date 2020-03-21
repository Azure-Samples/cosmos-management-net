using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_generated
{
    class Program
    {
        

        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Cosmos DB Management API Samples");

            await Run();

        }

        static async Task Run()
        {
            try
            {
                //=================================================================
                // Authenticate
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot config = builder.Build();
                string tenantId = config["tenantId"];
                string clientId = config["clientId"];
                string clientSecret = config["clientSecret"];
                string subscriptionId = config["subscriptionId"];

                CosmosDBManagementClient cosmosClient = await CreateCosmosClientAsync(clientId, clientSecret, tenantId, subscriptionId);

                //=================================================================
                // Create Resource Group
                string resourceGroupName = await CreateResourceGroupAsync(tenantId, clientId, clientSecret, subscriptionId);

                Console.WriteLine("Cosmos Database Account: Press any key to continue");
                Console.ReadKey();
                await Account(cosmosClient, resourceGroupName);

                Console.WriteLine("Cosmos DB Core(Sql) Resources: Press any key to continue");
                Console.ReadKey();
                await Sql(cosmosClient, resourceGroupName);

                Console.WriteLine("Cosmos DB MongoDB API Resources: Press any key to continue");
                Console.ReadKey();
                await MongoDB(cosmosClient, resourceGroupName);

                Console.WriteLine("Cosmos DB Cassandra API Resources: Press any key to continue");
                Console.ReadKey();
                await Cassandra(cosmosClient, resourceGroupName);

                Console.WriteLine("Cosmos DB Gremlin API Resources: Press any key to continue");
                Console.ReadKey();
                await Gremlin(cosmosClient, resourceGroupName);

                Console.WriteLine("Cosmos DB Table API Resources: Press any key to continue");
                Console.ReadKey();
                await Table(cosmosClient, resourceGroupName);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nPress any key to exit");
                Console.ReadKey();
            }
        }

        static async Task<CosmosDBManagementClient> CreateCosmosClientAsync(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {

            ServiceClientCredentials credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientSecret);

            CosmosDBManagementClient cosmosClient = new CosmosDBManagementClient(credentials);
            cosmosClient.SubscriptionId = subscriptionId;

            return cosmosClient;

        }

        static async Task<string> CreateResourceGroupAsync(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {            
            // Authenticate
            AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            //Create resource group
            string resourceGroupName = SdkContext.RandomResourceName("cosmos-", 10);

            await azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(Region.USWest2)
                .CreateAsync();
            

            return resourceGroupName;

        }

        static string RandomResourceName(string prefix)
        {
            string x = Path.GetRandomFileName();
            x = prefix + x.Replace(".", "");
            return x;
        }

        static async Task Account(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West US 2";
            string accountName = RandomResourceName("sql-");
            

            DatabaseAccount account = new DatabaseAccount();

            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Sql);
            await account.ListAccountsAsync(cosmosClient, resourceGroupName);
            await account.GetAccountAsync(cosmosClient, resourceGroupName, accountName);
            await account.ListKeysAsync(cosmosClient, resourceGroupName, accountName);
            await account.UpdateAccountAsync(cosmosClient, resourceGroupName, accountName);
            await account.AddRegionAsync(cosmosClient, resourceGroupName, accountName);
            await account.ChangeFailoverPriority(cosmosClient, resourceGroupName, accountName);
            await account.InitiateFailover(cosmosClient, resourceGroupName, accountName);

        }

        static async Task Sql(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West US 2";
            string accountName = RandomResourceName("sql-");
            string databaseName = "database1";
            string containerName = "container1";
            string partitionKey = "/myPartitionKey";
            int throughput = 400;
            int newThroughput = 500;
            int ttl = (60 * 60 * 24); // 1 day TTL

            string storedProcedureName = "storedProcedure1";
            string triggerName = "preTriggerAll1";
            string triggerType = "Pre";
            string triggerOperation = "All";
            string userDefinedFunctionName = "userDefinedFunction1";
            string storedProcedureBody = File.ReadAllText($@".\js\{storedProcedureName}.js");
            string triggerBody = File.ReadAllText($@".\js\{triggerName}.js");
            string userDefinedFunctionBody = File.ReadAllText($@".\js\{userDefinedFunctionName}.js");

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Sql);

            Sql sql = new Sql();

            //Database
            await sql.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName, throughput);
            await sql.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await sql.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await sql.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, newThroughput);

            //Container
            await sql.CreateContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, partitionKey, throughput);
            await sql.ListContainersAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await sql.GetContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName);
            await sql.UpdateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, newThroughput);
            await sql.UpdateContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, ttl);

            //Server-Side
            await sql.CreateStoredProcedureAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, storedProcedureName, storedProcedureBody);
            await sql.CreateTriggerAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, triggerName, triggerOperation, triggerType, triggerBody);
            await sql.CreateUserDefinedFunctionAsync(cosmosClient, resourceGroupName, accountName, databaseName, containerName, userDefinedFunctionName, userDefinedFunctionBody);
        }

        static async Task Gremlin(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West US 2";
            string accountName = RandomResourceName("gremlin-");
            string databaseName = "database1";
            string graphName = "graph1";
            string partitionKey = "/myPartitionKey";
            int throughput = 400;
            int newThroughput = 500;
            int ttl = (60 * 60 * 24); // 1 day TTL

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Gremlin);

            Gremlin gremlin = new Gremlin();

            //Database
            await gremlin.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName, throughput);
            await gremlin.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await gremlin.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await gremlin.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, newThroughput);

            //Graph
            await gremlin.CreateGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graphName, partitionKey, throughput);
            await gremlin.ListGraphsAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await gremlin.GetGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graphName);
            await gremlin.UpdateGraphThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, graphName, newThroughput); //intentionally set below min RU
            await gremlin.UpdateGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graphName, ttl);

        }

        static async Task MongoDB(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West Us 2";
            string accountName = RandomResourceName("mongodb-");
            string databaseName = "database1";
            string collectionName = "collection1";
            string shardKey = "myShardKey";
            int throughput = 400;
            int newThroughput = 500;

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.MongoDB);

            MongoDB mongoDB = new MongoDB();

            //Database
            await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName, throughput);
            await mongoDB.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await mongoDB.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await mongoDB.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, newThroughput);

            //Collection
            await mongoDB.CreateCollectionAsync(cosmosClient, resourceGroupName, accountName, databaseName, collectionName, shardKey, throughput);
            await mongoDB.ListCollectionsAsync(cosmosClient, resourceGroupName, accountName, databaseName);
            await mongoDB.GetCollectionAsync(cosmosClient, resourceGroupName, accountName, databaseName, collectionName);
            await mongoDB.UpdateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, collectionName, newThroughput);
            await mongoDB.UpdateCollectionAsync(cosmosClient, resourceGroupName, accountName, databaseName, collectionName);

        }

        static async Task Cassandra(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West US 2";
            string accountName = RandomResourceName("cassandra-");
            string keyspaceName = "keyspace1";
            string tableName = "table1";
            string partitionKey = "user_id";
            int throughput = 400;
            int newThroughput = 500;
            int ttl = (60 * 60 * 24); // 1 day TTL

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Cassandra);

            Cassandra cassandra = new Cassandra();

            //Keyspace
            await cassandra.CreateKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, throughput);
            await cassandra.ListKeyspacesAsync(cosmosClient, resourceGroupName, accountName);
            await cassandra.GetKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspaceName);
            await cassandra.UpdateKeyspaceThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, newThroughput);

            //Table
            await cassandra.CreateTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, tableName, partitionKey, throughput);
            await cassandra.ListTablesAsync(cosmosClient, resourceGroupName, accountName, keyspaceName);
            await cassandra.GetTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, tableName);
            await cassandra.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, tableName, newThroughput);
            await cassandra.UpdateTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, tableName, ttl);

        }

        static async Task Table(CosmosDBManagementClient cosmosClient, string resourceGroupName)
        {
            string location = "West Us 2";
            string accountName = RandomResourceName("table-");
            string tableName = "table1";
            int throughput = 400;
            int newThroughput = 500;

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Table);

            Table table = new Table();

            //Table
            await table.CreateTableAsync(cosmosClient, resourceGroupName, accountName, tableName, throughput);
            await table.ListTablesAsync(cosmosClient, resourceGroupName, accountName);
            await table.GetTableAsync(cosmosClient, resourceGroupName, accountName, tableName);
            await table.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, tableName, newThroughput);
            await table.UpdateTableAsync(cosmosClient, resourceGroupName, accountName, tableName);

        }
    }
}
