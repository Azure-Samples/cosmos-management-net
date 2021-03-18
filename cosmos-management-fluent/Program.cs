using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Network.Fluent;
using System.IO;

namespace cosmos_management_fluent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Cosmos DB Fluent Management API Samples");

            await Run();
        }

        static async Task Run()
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
            AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            //Create a resource group to put resources into
            Console.WriteLine("Create a resource group: Press any key to continue");
            Console.ReadKey();
            string resourceGroupName = await CreateResourceGroupAsync(azure);

            Console.WriteLine("Cosmos Database Account: Press any key to continue");
            Console.ReadKey();
            await DatabaseAccount(azure, resourceGroupName);

            Console.WriteLine("Cosmos DB Core(Sql) Resources: Press any key to continue");
            Console.ReadKey();
            await Sql(azure, resourceGroupName);

            Console.WriteLine("Cosmos DB MongoDB API Resources: Press any key to continue");
            Console.ReadKey();
            await MongoDB(azure, resourceGroupName);

            Console.WriteLine("Cosmos DB Cassandra API Resources: Press any key to continue");
            Console.ReadKey();
            await Cassandra(azure, resourceGroupName);

            Console.WriteLine("Cosmos DB Gremlin API Resources: Press any key to continue");
            Console.ReadKey();
            await Gremlin(azure, resourceGroupName);

            Console.WriteLine("Cosmos DB Table API Resources: Press any key to continue");
            Console.ReadKey();
            await Table(azure, resourceGroupName);
        }

        static async Task<string> CreateResourceGroupAsync(IAzure azure)
        {
            string resourceGroupName = SdkContext.RandomResourceName("cosmos-", 10);

            await azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(Region.USWest2)
                .CreateAsync();

            return resourceGroupName;
        }

        static async Task DatabaseAccount(IAzure azure, string resourceGroupName)
        {
            DatabaseAccount databaseAccount = new DatabaseAccount();

            string accountName = SdkContext.RandomResourceName("sql-", 10);

            await databaseAccount.CreateDatabaseAccountSqlAsync(azure, resourceGroupName, accountName);
            await databaseAccount.ListDatabaseAccountsAsync(azure, resourceGroupName);
            await databaseAccount.GetAccountAsync(azure, resourceGroupName, accountName);
            await databaseAccount.ListKeysAsync(azure, resourceGroupName, accountName);
            await databaseAccount.AddRegionAsync(azure, resourceGroupName, accountName); //Add South Central US
            await databaseAccount.ChangeFailoverPriorityAsync(azure, resourceGroupName, accountName); //Swap East US 2 and South Central US
            await databaseAccount.InitiateFailoverAsync(azure, resourceGroupName, accountName); //Make East US 2 write region
            await databaseAccount.RemoveRegionAsync(azure, resourceGroupName, accountName); //Remove South Central US region
            INetwork virtualNetwork = await databaseAccount.CreateVirtualNetworkAsync(azure, resourceGroupName, accountName);
            await databaseAccount.UpdateAccountAddVirtualNetworkAsync(azure, resourceGroupName, accountName, virtualNetwork);
        }

        static async Task Sql(IAzure azure, string resourceGroupName)
        {
            Sql sql = new Sql();

            //Note: Cosmos account names must be lower case
            string accountName = SdkContext.RandomResourceName("sql-", 10);
            string databaseName1 = "database1";
            string databaseName2 = "database2";
            string containerName = "container1";

            await sql.CreateContainerAllAsync(azure, resourceGroupName, accountName, databaseName1, containerName);
            await sql.GetContainerAsync(azure, resourceGroupName, accountName, databaseName1, containerName);
            await sql.AddDatabaseToAccountAsync(azure, resourceGroupName, accountName, databaseName2);
            await sql.AddContainerToDatabaseAsync(azure, resourceGroupName, accountName, databaseName2, containerName, 400);
            await sql.UpdateContainerThroughputAsync(azure, resourceGroupName, accountName, databaseName1, containerName, 500);
            await sql.UpdateContainerAsync(azure, resourceGroupName, accountName, databaseName1, containerName);

        }

        static async Task MongoDB(IAzure azure, string resourceGroupName)
        {
            MongoDB mongoDB = new MongoDB();

            //Note: Cosmos account names must be lower case
            string accountName = SdkContext.RandomResourceName("mongodb-", 10);
            string databaseName = "database1";
            string collectionName = "collection1";

            
            await mongoDB.CreateCollectionAsync(azure, resourceGroupName, accountName, databaseName, collectionName);
            await mongoDB.UpdateCollectionThroughputAsync(azure, resourceGroupName, accountName, databaseName, collectionName, 500);
            //await mongoDB.UpdateCollectionAsync(azure, resourceGroupName, accountName, databaseName, collectionName);
        }

        static async Task Cassandra(IAzure azure, string resourceGroupName)
        {
            Cassandra cassandra = new Cassandra();

            //Note: Cosmos account names must be lower case
            string accountName = SdkContext.RandomResourceName("cassandra-", 10);
            string keyspaceName = "keyspace1";
            string tableName = "table1";

            await cassandra.CreateTableAsync(azure, resourceGroupName, accountName, keyspaceName, tableName);
            await cassandra.UpdateTableThroughputAsync(azure, resourceGroupName, accountName, keyspaceName, tableName, 500);
            await cassandra.UpdateTableAsync(azure, resourceGroupName, accountName, keyspaceName, tableName);
        }

        static async Task Gremlin(IAzure azure, string resourceGroupName)
        {
            Gremlin gremlin = new Gremlin();

            //Note: Cosmos account names must be lower case
            string accountName = SdkContext.RandomResourceName("gremlin-", 10);
            string databaseName = "database1";
            string graphName = "graph1";

            await gremlin.CreateGraphAsync(azure, resourceGroupName, accountName, databaseName, graphName);
            await gremlin.UpdateGraphThroughputAsync(azure, resourceGroupName, accountName, databaseName, graphName, 500);
            await gremlin.UpdateGraphAsync(azure, resourceGroupName, accountName, databaseName, graphName);
        }
        
        static async Task Table(IAzure azure, string resourceGroupName)
        {
            Table table = new Table();

            //Note: Cosmos account names must be lower case
            string accountName = SdkContext.RandomResourceName("table-", 10);
            string tableName = "table1";

            await table.CreateTableAsync(azure, resourceGroupName, accountName, tableName);
            await table.UpdateTableThroughputAsync(azure, resourceGroupName, accountName, tableName, 500);

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
