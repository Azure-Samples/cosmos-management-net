using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;

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
                //Load secrets
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<Secrets>();

                IConfigurationRoot config = builder.Build();

                string tenantId = config.GetSection("TenantId").Value;
                string clientId = config.GetSection("ClientId").Value;
                string clientSecret = config.GetSection("ClientSecret").Value;
                string subscriptionId = config.GetSection("SubscriptionId").Value;

                //Authenticate
                ServiceClientCredentials credentials = await AuthenticateAsync(clientId, clientSecret, tenantId);

                string location = "West US 2";

                //=================================================================
                // Create Resource Group
                string resourceGroupName = await CreateResourceGroupAsync(credentials, subscriptionId, location);

                //Cosmos Management Client
                CosmosDBManagementClient cosmosClient = CreateCosmosClient(credentials, subscriptionId);

                Console.WriteLine("Cosmos Database Account: Press any key to continue");
                Console.ReadKey();
                await Account(cosmosClient, resourceGroupName, location);

                Console.WriteLine("Cosmos DB Core(Sql) Resources: Press any key to continue");
                Console.ReadKey();
                await Sql(cosmosClient, resourceGroupName, location);

                Console.WriteLine("Cosmos DB MongoDB API Resources: Press any key to continue");
                Console.ReadKey();
                //await MongoDB(cosmosClient, resourceGroupName, location);

                Console.WriteLine("Cosmos DB Cassandra API Resources: Press any key to continue");
                Console.ReadKey();
                await Cassandra(cosmosClient, resourceGroupName, location);

                Console.WriteLine("Cosmos DB Gremlin API Resources: Press any key to continue");
                Console.ReadKey();
                await Gremlin(cosmosClient, resourceGroupName, location);

                Console.WriteLine("Cosmos DB Table API Resources: Press any key to continue");
                Console.ReadKey();
                await Table(cosmosClient, resourceGroupName, location);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nPress any key to exit");
                Console.ReadKey();
            }
        }

        static async Task<ServiceClientCredentials> AuthenticateAsync(string clientId, string clientSecret, string tenantId)
        {
            ServiceClientCredentials credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientSecret);

            return credentials;
        }

        static CosmosDBManagementClient CreateCosmosClient(ServiceClientCredentials credentials, string subscriptionId)
        {

            CosmosDBManagementClient cosmosClient = new CosmosDBManagementClient(credentials)
            {
                SubscriptionId = subscriptionId
            };

            return cosmosClient;

        }

        static async Task<string> CreateResourceGroupAsync(ServiceClientCredentials credentials, string subscriptionId, string location)
        {

            ResourceManagementClient resourceManagementClient = new ResourceManagementClient(credentials)
            {
                SubscriptionId = subscriptionId
            };

            string resourceGroupName = RandomResourceName("cosmos-");

            await resourceManagementClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, new ResourceGroup(location));

            return resourceGroupName;

        }

        static string RandomResourceName(string prefix)
        {
            string x = Path.GetRandomFileName();
            x = prefix + x.Replace(".", "");
            return x;
        }

        static async Task Account(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
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

        static async Task Sql(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("sql-");
            string database1Name = "database1";
            string database2Name = "database2";
            string database3Name = "database3";
            string container1Name = "container1";
            string container2Name = "container2";
            string partitionKey = "/myPartitionKey";
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 4000;
            int newAutoscaleMaxThroughput = 5000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL


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

            //Database (shared throughput)
            await sql.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name, throughput); //standard throughput
            await sql.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name, autoscaleMaxThroughput, autoScale: true); //autoscale throughput
            await sql.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await sql.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name);
            await sql.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, newThroughput); //update standard throughput
            await sql.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database2Name, newAutoscaleMaxThroughput, autoScale: true); //update autoscale throughput
            await sql.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: true); //migrate manual to autoscale
            await sql.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: false); //migrate autoscale to manual

            //Container (dedicated throughput)
            await sql.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database3Name);
            await sql.CreateContainerAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, partitionKey, throughput); //manual throughput
            await sql.CreateContainerAsync(cosmosClient, resourceGroupName, accountName, database3Name, container2Name, partitionKey, autoscaleMaxThroughput, autoScale: true); //autoscale throughput
            await sql.ListContainersAsync(cosmosClient, resourceGroupName, accountName, database3Name);
            await sql.GetContainerAsync(cosmosClient, resourceGroupName, accountName, database3Name, container2Name);
            await sql.UpdateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, newThroughput); //update standard throughput
            await sql.UpdateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, container2Name, newAutoscaleMaxThroughput, autoScale: true); //update autoscale throughput
            await sql.MigrateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, autoScale: true); //migrate manual to autoscale
            await sql.MigrateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, autoScale: false); //migrate autoscale to manual
            await sql.UpdateContainerAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, defaultTtl: updatedTtl);

            //Server-Side
            await sql.CreateStoredProcedureAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, storedProcedureName, storedProcedureBody);
            await sql.CreateTriggerAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, triggerName, triggerOperation, triggerType, triggerBody);
            await sql.CreateUserDefinedFunctionAsync(cosmosClient, resourceGroupName, accountName, database3Name, container1Name, userDefinedFunctionName, userDefinedFunctionBody);
        }

        static async Task Gremlin(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("gremlin-");
            string database1Name = "database1";
            string database2Name = "database2";
            string database3Name = "database3";
            string graph1Name = "graph1";
            string graph2Name = "graph2";
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 4000;
            int newAutoscaleMaxThroughput = 5000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Gremlin);

            Gremlin gremlin = new Gremlin();

            //Database (shared throughput)
            await gremlin.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name, throughput); //standard throughput
            await gremlin.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await gremlin.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await gremlin.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name);
            await gremlin.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, newThroughput);//standard throughput
            await gremlin.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await gremlin.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: true); //migrate standard to autoscale
            await gremlin.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: false); //migrate autoscale to standard

            //Graph (dedicated throughput)
            await gremlin.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database3Name);//dedicated database
            await gremlin.CreateGraphAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph1Name, throughput); //standard throughput
            await gremlin.CreateGraphAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph2Name, autoscaleMaxThroughput, autoScale: true); //autoscale throughput
            await gremlin.ListGraphsAsync(cosmosClient, resourceGroupName, accountName, database3Name);
            await gremlin.GetGraphAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph2Name);
            await gremlin.UpdateGraphThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph1Name, newThroughput);//standard throughput
            await gremlin.UpdateGraphThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await gremlin.MigrateGraphThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph1Name, autoScale: true); //migrate standard to autoscale
            await gremlin.MigrateGraphThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph1Name, autoScale: false); //migrate autoscale to standard
            await gremlin.UpdateGraphAsync(cosmosClient, resourceGroupName, accountName, database3Name, graph1Name, defaultTtl: updatedTtl);

        }

        static async Task MongoDB(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("mongodb-");
            string database1Name = "database1";
            string database2Name = "database2";
            string database3Name = "database3";
            string collection1Name = "collection1";
            string collection2Name = "collection2";
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 4000;
            int newAutoscaleMaxThroughput = 5000;

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.MongoDB);

            MongoDB mongoDB = new MongoDB();

            //Database (shared throughput)
            await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name, throughput);//standard throughput
            await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await mongoDB.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
            await mongoDB.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name);
            await mongoDB.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, newThroughput);//standard throughput
            await mongoDB.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await mongoDB.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: true); //migrate standard to autoscale
            await mongoDB.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: false); //migrate autoscale to standard

            //Collection (dedicated throughput)
            await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database3Name);//dedicated collection throughput
            await mongoDB.CreateCollectionAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection1Name, throughput);//standard throughput
            await mongoDB.CreateCollectionAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await mongoDB.ListCollectionsAsync(cosmosClient, resourceGroupName, accountName, database3Name);
            await mongoDB.GetCollectionAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection2Name);
            await mongoDB.UpdateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection1Name, newThroughput);//standard throughput
            await mongoDB.UpdateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await mongoDB.MigrateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection1Name, autoScale: true); //migrate standard to autoscale
            await mongoDB.MigrateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection1Name, autoScale: false); //migrate autoscale to standard
            await mongoDB.UpdateCollectionAsync(cosmosClient, resourceGroupName, accountName, database3Name, collection1Name);

        }

        static async Task Cassandra(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("cassandra-");
            string keyspace1Name = "keyspace1";
            string keyspace2Name = "keyspace2";
            string keyspace3Name = "keyspace3";
            string table1Name = "table1";
            string table2Name = "table2";
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 4000;
            int newAutoscaleMaxThroughput = 5000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Cassandra);

            Cassandra cassandra = new Cassandra();

            //Keyspace (shared throughput)
            await cassandra.CreateKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspace1Name, throughput);//standard throughput
            await cassandra.CreateKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspace2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await cassandra.ListKeyspacesAsync(cosmosClient, resourceGroupName, accountName);
            await cassandra.GetKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspace2Name);
            await cassandra.UpdateKeyspaceThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace1Name, newThroughput);//standard throughput
            await cassandra.UpdateKeyspaceThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await cassandra.MigrateKeyspaceThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace1Name, autoScale: true); //migrate standard to autoscale
            await cassandra.MigrateKeyspaceThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace1Name, autoScale: false); //migrate autoscale to standard

            //Table (dedicated throughput)
            await cassandra.CreateKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name);//dedicated keyspace
            await cassandra.CreateTableAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table1Name, throughput);//standard throughput
            await cassandra.CreateTableAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await cassandra.ListTablesAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name);
            await cassandra.GetTableAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table2Name);
            await cassandra.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table1Name, newThroughput);//standard throughput
            await cassandra.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table2Name, newAutoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await cassandra.MigrateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table1Name, autoScale: true); //migrate standard to autoscale
            await cassandra.MigrateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table1Name, autoScale: false); //migrate autoscale to standard
            await cassandra.UpdateTableAsync(cosmosClient, resourceGroupName, accountName, keyspace3Name, table1Name, defaultTtl: updatedTtl);

        }

        static async Task Table(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("table-");
            string table1Name = "table1";
            string table2Name = "table2";
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 4000;
            int newAutoscaleMaxThroughput = 5000;

            //Create a new account
            DatabaseAccount account = new DatabaseAccount();
            await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Table);

            Table table = new Table();

            //Table
            await table.CreateTableAsync(cosmosClient, resourceGroupName, accountName, table1Name, throughput);//standard throughput
            await table.CreateTableAsync(cosmosClient, resourceGroupName, accountName, table2Name, autoscaleMaxThroughput, autoScale: true);//autoscale throughput
            await table.ListTablesAsync(cosmosClient, resourceGroupName, accountName);
            await table.GetTableAsync(cosmosClient, resourceGroupName, accountName, table2Name);
            await table.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, table1Name, newThroughput);//standard throughput
            await table.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, table2Name, newAutoscaleMaxThroughput, autoScale: true);//standard throughput
            await table.MigrateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, table1Name, autoScale: true); //migrate standard to autoscale
            await table.MigrateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, table1Name, autoScale: false); //migrate autoscale to standard
            await table.UpdateTableAsync(cosmosClient, resourceGroupName, accountName, table1Name);

        }
    }

    class Secrets
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }

    }
}
