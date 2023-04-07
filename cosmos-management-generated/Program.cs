using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.CosmosDB;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using System.Collections.Generic;
using Microsoft.Azure.Management.CosmosDB.Models;
using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace cosmos_management_generated
{
    class Program
    {
        #pragma warning disable CS8618  //Suppress non-nullable fields below

        private static IConfigurationRoot _config;
        private static string _subscriptionId;
        private static ServiceClientCredentials _credentials;
        private static string _location;
        private static string _resourceGroupName;
        private static CosmosDBManagementClient _cosmosClient;

        static async Task Main(string[] args)
        {
            //=================================================================
            //Load secrets
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Secrets>();

            _config = builder.Build();

            await MainMenu();

        }

        static async Task MainMenu()
        {
            try
            {
                bool exit = false;

                while (exit == false)
                {
                    Console.Clear();
                    Console.WriteLine($"Azure Cosmos DB Management API Samples");
                    Console.WriteLine($"--------------------------------------");
                    Console.WriteLine($"[a]   Authenticate");
                    Console.WriteLine($"[b]   Create Cosmos Management Client");
                    Console.WriteLine($"[c]   Set Region for ARM account resource");
                    Console.WriteLine($"[d]   Set or Create a Resource Group");
                    Console.WriteLine($"--------------------------------------");
                    Console.WriteLine($"[e]   Database Account Operations");
                    Console.WriteLine($"[f]   Cassandra API Operations");
                    Console.WriteLine($"[g]   Gremlin API Operations");
                    Console.WriteLine($"[h]   MongoDB API Operations");
                    Console.WriteLine($"[i]   NoSQL API Operations");
                    Console.WriteLine($"[j]   Table API Operations");
                    Console.WriteLine($"[x]   Exit");

                    ConsoleKeyInfo result = Console.ReadKey(true);

                    if (result.KeyChar == 'a')
                    {
                        Console.Clear();
                        await AuthenticateAsync();
                    }
                    else if (result.KeyChar == 'b')
                    {
                        Console.Clear();
                        _cosmosClient = CreateCosmosClient(_credentials, _subscriptionId);
                    }
                    else if (result.KeyChar == 'c')
                    {
                        Console.Clear();
                        SetRegion();
                    }
                    else if (result.KeyChar == 'd')
                    {
                        Console.Clear();
                        await SetResourceGroupAsync();
                    }
                    else if (result.KeyChar == 'e')
                    {
                        Console.Clear();
                        await Account(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'f')
                    {
                        Console.Clear();
                        await Cassandra(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'g')
                    {
                        Console.Clear();
                        await Gremlin(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'h')
                    {
                        Console.Clear();
                        await MongoDB(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'i')
                    {
                        Console.Clear();
                        await NoSql(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'j')
                    {
                        Console.Clear();
                        await Table(_cosmosClient, _resourceGroupName, _location);
                    }
                    else if (result.KeyChar == 'x')
                    {
                        exit = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\nPress any key to exit");
                Console.ReadKey();
            }
        }

        static void SetRegion()
        {
            Console.WriteLine("Enter region to use:");
            _location = Console.ReadLine();
        }

        static async Task SetResourceGroupAsync()
        {
            Console.WriteLine("Enter existing resource group name or leave blank to generate one with a random name");
            _resourceGroupName = Console.ReadLine();

            if(_resourceGroupName.Length == 0)
            {
                _resourceGroupName = await CreateResourceGroupAsync(_credentials, _subscriptionId, _location);
            }
        }

        static async Task AuthenticateAsync()
        {
            string tenantId = _config.GetSection("tenantId").Value;
            string appId = _config.GetSection("appId").Value;
            string password = _config.GetSection("password").Value;
            _subscriptionId = _config.GetSection("subscriptionId").Value;

            //Authenticate
            _credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, appId, password);

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

        static string RandomResourceName(string prefix = "")
        {
            string x = Path.GetRandomFileName();
            x = prefix + x.Replace(".", "");
            return x;
        }

        static async Task Account(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = "";
            DatabaseAccount.Api apiType = DatabaseAccount.Api.Sql;
            
            DatabaseAccount account = new DatabaseAccount();

            bool exit = false;

            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Database Account Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Set Account Name");
                Console.WriteLine($"[b]   Set database API");
                Console.WriteLine($"[c]   Create a new Cosmos DB Account");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[d]   List accounts in Resource Group");
                Console.WriteLine($"[e]   Get a Cosmos DB Account");
                Console.WriteLine($"[f]   List Account Keys");
                Console.WriteLine($"[g]   Update a Cosmos DB Account");
                Console.WriteLine($"[h]   Add a Region");
                Console.WriteLine($"[i]   Change Failover Priority");
                Console.WriteLine($"[j]   Initiate a Failover");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    Console.WriteLine("Enter Cosmos DB Account Name. Leave blank to generate a random name");
                    
                    accountName = Console.ReadLine();

                    if (accountName.Length == 0)
                    {
                        accountName = RandomResourceName();
                    }
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    Console.WriteLine("Enter Database API. (sql,gremlin,cassandra,mongodb,table)");
                    string api = Console.ReadLine();

                    switch (api)
                    {
                        case "sql":
                            apiType = DatabaseAccount.Api.Sql;
                            break;
                        case "gremlin":
                            apiType = DatabaseAccount.Api.Gremlin;
                            break;
                        case "cassandra":
                            apiType = DatabaseAccount.Api.Cassandra;
                            break;
                        case "mongodb":
                            apiType = DatabaseAccount.Api.MongoDB;
                            break;
                        case "table":
                            apiType = DatabaseAccount.Api.Table;
                            break;
                    }

                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, apiType);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    List<string> accounts = await account.ListAccountsAsync(cosmosClient, resourceGroupName);

                    foreach(string acct in accounts)
                    {
                        Console.WriteLine($"Account Name: {acct}");
                    }

                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();

                    string acct = await SelectAccount(account, resourceGroupName);

                    Console.Clear();

                    await account.GetAccountAsync(cosmosClient, resourceGroupName, acct);

                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();

                    accountName = await SelectAccount(account, resourceGroupName);

                    await account.ListKeysAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();

                    accountName = await SelectAccount(account, resourceGroupName);

                    await account.UpdateAccountAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();

                    accountName = await SelectAccount(account, resourceGroupName);

                    //Add some code in here to get the current regions and prompt to add one.

                    await account.AddRegionAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();

                    accountName = await SelectAccount(account, resourceGroupName);

                    //Add some code in here to get the current regions and prompt to change priority.

                    await account.ChangeFailoverPriority(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'j')
                {
                    Console.Clear();

                    accountName = await SelectAccount(account, resourceGroupName);

                    //Add some code in here to get the current regions and prompt to failover region 0.

                    await account.InitiateFailover(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        static async Task<string> SelectAccount(DatabaseAccount account, string resourceGroupName)
        {
            Console.Clear();

            List<string> accounts = await account.ListAccountsAsync(_cosmosClient, resourceGroupName);

            int i = 0;
            Console.WriteLine($"Enter number for Cosmos account, press Enter");
            foreach (string acct in accounts)
            {
                Console.WriteLine($"{i}\tAccount Name: {acct}");
                i++;
            }
            int s = Convert.ToInt32(Console.ReadLine());

            return accounts[s];
        }

        static async Task NoSql(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("sql-");
            string databaseName = "database1";
            string container1Name = "container1";
            string container2Name = "container2";
            List<string> partitionKey = new List<string> { "/myPartitionKey" };
            List<string> hierarchicalPk = new List<string> { "/tenantId", "/departmentId", "/employeeId" };
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 1000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL

            string storedProcedureName = "storedProcedure1";
            string triggerName = "preTriggerAll1";
            string triggerType = "Pre";
            string triggerOperation = "All";
            string userDefinedFunctionName = "userDefinedFunction1";
            string storedProcedureBody = File.ReadAllText($@".\js\{storedProcedureName}.js");
            string triggerBody = File.ReadAllText($@".\js\{triggerName}.js");
            string userDefinedFunctionBody = File.ReadAllText($@".\js\{userDefinedFunctionName}.js");

            NoSql sql = new NoSql();

            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"SQL API Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Create SQL API Account");
                Console.WriteLine($"[b]   Create Database");
                Console.WriteLine($"[c]   List all Databases in Account");
                Console.WriteLine($"[d]   Get Database in Account");
                Console.WriteLine($"[e]   Create Container 1 PK standard throughput");
                Console.WriteLine($"[f]   Create Container hierarchical PK, autoscale throughput ");
                Console.WriteLine($"[g]   List all Containers in Database");
                Console.WriteLine($"[h]   Get Container in Database");
                Console.WriteLine($"[i]   Update container standard throughput");
                Console.WriteLine($"[j]   Migrate container throughput from standard to autoscale");
                Console.WriteLine($"[k]   Update Container");
                Console.WriteLine($"[l]   Create Stored Procedure");
                Console.WriteLine($"[m]   Create Trigger");
                Console.WriteLine($"[n]   Create User Defined Function");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    //Create a new account
                    DatabaseAccount account = new DatabaseAccount();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Sql);
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    //Create a database
                    await sql.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await sql.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await sql.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    //manual throughput, one partition key
                    await sql.CreateContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, partitionKey, throughput);
                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();
                    //autoscale throughput, hierarchical partition key
                    await sql.CreateContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, container2Name, hierarchicalPk, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();
                    await sql.ListContainersAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();
                    await sql.GetContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, container2Name);
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();
                    //update standard throughput
                    await sql.UpdateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, newThroughput);
                }
                else if (result.KeyChar == 'j')
                {
                    Console.Clear();
                    //migrate manual to autoscale
                    await sql.MigrateContainerThroughputAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, autoScale: true);
                }
                else if (result.KeyChar == 'k')
                {
                    Console.Clear();
                    //update container ttl
                    await sql.UpdateContainerAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, defaultTtl: updatedTtl);
                }
                else if (result.KeyChar == 'l')
                {
                    Console.Clear();
                    await sql.CreateStoredProcedureAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, storedProcedureName, storedProcedureBody);
                }
                else if (result.KeyChar == 'm')
                {
                    Console.Clear();
                    await sql.CreateTriggerAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, triggerName, triggerOperation, triggerType, triggerBody);
                }
                else if (result.KeyChar == 'n')
                {
                    Console.Clear();
                    await sql.CreateUserDefinedFunctionAsync(cosmosClient, resourceGroupName, accountName, databaseName, container1Name, userDefinedFunctionName, userDefinedFunctionBody);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }
        
        static async Task Gremlin(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("gremlin-");
            string databaseName = "database1";
            string graph1Name = "graph1";
            string graph2Name = "graph2";
            List<string> partitionKey = new List<string> { "/myPartitionKey" };
            int throughput = 400;
            int newThroughput = 500;
            int autoscaleMaxThroughput = 1000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL

            Gremlin gremlin = new Gremlin();

            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Gremlin API Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Create Gremlin API Account");
                Console.WriteLine($"[b]   Create Database");
                Console.WriteLine($"[c]   List all Databases in Account");
                Console.WriteLine($"[d]   Get Database in Account");
                Console.WriteLine($"[e]   Create Graph with standard throughput");
                Console.WriteLine($"[f]   Create Graph with autoscale throughput ");
                Console.WriteLine($"[g]   List all Graphs in Database");
                Console.WriteLine($"[h]   Get Graph in Database");
                Console.WriteLine($"[i]   Update Graph");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    //Create a new account
                    DatabaseAccount account = new DatabaseAccount();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Gremlin);
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    //Create a database
                    await gremlin.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await gremlin.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await gremlin.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    //standard throughput
                    await gremlin.CreateGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graph1Name, partitionKey, throughput);
                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();
                    //autoscale throughput
                    await gremlin.CreateGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graph2Name, partitionKey, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();
                    await gremlin.ListGraphsAsync(cosmosClient, resourceGroupName, accountName, databaseName);
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();
                    await gremlin.GetGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graph2Name);
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();
                    //update graph ttl
                    await gremlin.UpdateGraphAsync(cosmosClient, resourceGroupName, accountName, databaseName, graph1Name, defaultTtl: updatedTtl);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        static async Task MongoDB(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("mongodb-");
            string database1Name = "database1";
            string database2Name = "database2";
            string collection1Name = "collection1";
            string collection2Name = "collection2";
            int autoscaleMaxThroughput = 1000;
            int newAutoscaleMaxThroughput = 2000;

            MongoDB mongoDB = new MongoDB();

            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"MongoDB API Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Create MongoDB API Account");
                Console.WriteLine($"[b]   Create Database with shared autoscale throughput");
                Console.WriteLine($"[c]   Create Database with no throughput");
                Console.WriteLine($"[d]   List all Databases in Account");
                Console.WriteLine($"[e]   Get Database in Account");
                Console.WriteLine($"[f]   Update Database with autoscale throughput");
                Console.WriteLine($"[g]   Migrate shared throughput Database from autoscale to standard throughput");
                Console.WriteLine($"[h]   Create Collection in database using shared throughput");
                Console.WriteLine($"[i]   Create Collection with dedicated autoscale throughput");
                Console.WriteLine($"[j]   List all Collections in Database");
                Console.WriteLine($"[k]   Get Collection in Database");
                Console.WriteLine($"[j]   Update Collection with autoscale throughput");
                Console.WriteLine($"[k]   Migrate Collection from autoscale to standard throughput");
                Console.WriteLine($"[l]   Update Collection");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    //Create a new account
                    DatabaseAccount account = new DatabaseAccount();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.MongoDB);
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    //Create a database with shared autoscale throughput
                    await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    //database with no throughput
                    await mongoDB.CreateDatabaseAsync(cosmosClient, resourceGroupName, accountName, database2Name);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await mongoDB.ListDatabasesAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    await mongoDB.GetDatabaseAsync(cosmosClient, resourceGroupName, accountName, database1Name);
                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();
                    //Update database autoscale throughput
                    await mongoDB.UpdateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, newAutoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();
                    //migrate shared throughput database from autoscale to standard throughput 
                    await mongoDB.MigrateDatabaseThroughputAsync(cosmosClient, resourceGroupName, accountName, database1Name, autoScale: false);
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();
                    //collection to use throughput from database
                    await mongoDB.CreateCollectionAsync(cosmosClient, resourceGroupName, accountName, database1Name, collection1Name);
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();
                    //collection with dedicated autoscale throughput
                    await mongoDB.CreateCollectionAsync(cosmosClient, resourceGroupName, accountName, database2Name, collection2Name, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'j')
                {
                    Console.Clear();
                    await mongoDB.ListCollectionsAsync(cosmosClient, resourceGroupName, accountName, database2Name);
                }
                else if (result.KeyChar == 'k')
                {
                    Console.Clear();
                    await mongoDB.GetCollectionAsync(cosmosClient, resourceGroupName, accountName, database2Name, collection2Name);
                }
                else if (result.KeyChar == 'l')
                {
                    Console.Clear();
                    //Update autoscale collection with additional throughput
                    await mongoDB.UpdateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database2Name, collection2Name, newAutoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'm')
                {
                    Console.Clear();
                    //migrate autoscale to standard
                    await mongoDB.MigrateCollectionThroughputAsync(cosmosClient, resourceGroupName, accountName, database2Name, collection2Name, autoScale: false);
                }
                else if (result.KeyChar == 'n')
                {
                    Console.Clear();
                    await mongoDB.UpdateCollectionAsync(cosmosClient, resourceGroupName, accountName, database2Name, collection2Name);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        static async Task Cassandra(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("cassandra-");
            string keyspaceName = "keyspace1";
            string table1Name = "table1";
            string table2Name = "table2";
            int throughput = 400;
            int autoscaleMaxThroughput = 1000;
            int newAutoscaleMaxThroughput = 2000;
            int updatedTtl = (60 * 60 * 24); // 1 day TTL

            Cassandra cassandra = new Cassandra();

            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Cassandra API Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Create Cassandra API Account");
                Console.WriteLine($"[b]   Create Keyspace");
                Console.WriteLine($"[c]   List all Keyspaces in Account");
                Console.WriteLine($"[d]   Get Keyspace in Account");
                Console.WriteLine($"[e]   Create Table with autoscale throughput ");
                Console.WriteLine($"[f]   List all Tables in Keyspace");
                Console.WriteLine($"[g]   Get Table in Keyspace");
                Console.WriteLine($"[h]   Update Table with autoscale throughput");
                Console.WriteLine($"[i]   Migrate Table throughput from standard to autoscale");
                Console.WriteLine($"[j]   Update Table");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    //Create a new account
                    DatabaseAccount account = new DatabaseAccount();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Cassandra);
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    //Create a keyspace
                    await cassandra.CreateKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspaceName);
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await cassandra.ListKeyspacesAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await cassandra.GetKeyspaceAsync(cosmosClient, resourceGroupName, accountName, keyspaceName);
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    //autoscale throughput
                    await cassandra.CreateTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, table2Name, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'f')
                {
                    Console.Clear();
                    await cassandra.ListTablesAsync(cosmosClient, resourceGroupName, accountName, keyspaceName);
                }
                else if (result.KeyChar == 'g')
                {
                    Console.Clear();
                    await cassandra.GetTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, table2Name);
                }
                else if (result.KeyChar == 'h')
                {
                    Console.Clear();
                    //Update table with autoscale throughput
                    await cassandra.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, table2Name, newAutoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'i')
                {
                    Console.Clear();
                    //migrate standard to autoscale
                    await cassandra.MigrateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, table1Name, autoScale: true);
                }
                else if (result.KeyChar == 'j')
                {
                    Console.Clear();
                    //update table ttl
                    await cassandra.UpdateTableAsync(cosmosClient, resourceGroupName, accountName, keyspaceName, table1Name, defaultTtl: updatedTtl);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
        }

        static async Task Table(CosmosDBManagementClient cosmosClient, string resourceGroupName, string location)
        {
            string accountName = RandomResourceName("table-");
            string table2Name = "table2";
            int autoscaleMaxThroughput = 1000;
            int newAutoscaleMaxThroughput = 2000;

            Table table = new Table();

            bool exit = false;
            while (exit == false)
            {
                Console.Clear();
                Console.WriteLine($"Table API Management Samples");
                Console.WriteLine($"-----------------------------------");
                Console.WriteLine($"[a]   Create Table API Account");
                Console.WriteLine($"[b]   Create Table with autoscale throughput ");
                Console.WriteLine($"[c]   List all Tables in Account");
                Console.WriteLine($"[d]   Get Table in Account");
                Console.WriteLine($"[e]   Update Table's autoscale throughput");
                Console.WriteLine($"[x]   Return to Main Menu");

                ConsoleKeyInfo result = Console.ReadKey(true);

                if (result.KeyChar == 'a')
                {
                    Console.Clear();
                    //Create a new account
                    DatabaseAccount account = new DatabaseAccount();
                    await account.CreateAccountAsync(cosmosClient, resourceGroupName, location, accountName, DatabaseAccount.Api.Table);
                }
                else if (result.KeyChar == 'b')
                {
                    Console.Clear();
                    //create table with autoscale throughput
                    await table.CreateTableAsync(cosmosClient, resourceGroupName, accountName, table2Name, autoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'c')
                {
                    Console.Clear();
                    await table.ListTablesAsync(cosmosClient, resourceGroupName, accountName);
                }
                else if (result.KeyChar == 'd')
                {
                    Console.Clear();
                    await table.GetTableAsync(cosmosClient, resourceGroupName, accountName, table2Name);
                }
                else if (result.KeyChar == 'e')
                {
                    Console.Clear();
                    //update amount of table's autoscale throughput
                    await table.UpdateTableThroughputAsync(cosmosClient, resourceGroupName, accountName, table2Name, newAutoscaleMaxThroughput, autoScale: true);
                }
                else if (result.KeyChar == 'x')
                {
                    exit = true;
                }
            }
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
