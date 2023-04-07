using System;
using Microsoft.Azure.Management.CosmosDB;
using System.Threading.Tasks;
using Microsoft.Azure.Management.CosmosDB.Models;
using System.Collections.Generic;

namespace cosmos_management_generated
{
	public class DatabaseAccount
    {

		public enum Api
		{
			Sql,
			MongoDB,
			Cassandra,
			Gremlin,
			Table
		}

		public async Task RegenerateKeyAsync(CosmosDBManagementClient cosmosClient, string resourceGroupName, string accountName)
        {
			DatabaseAccountRegenerateKeyParameters keyParameters = new DatabaseAccountRegenerateKeyParameters("primary");
			await cosmosClient.DatabaseAccounts.RegenerateKeyAsync(resourceGroupName, accountName, keyParameters);
        }

		public async Task<DatabaseAccountGetResults> CreateAccountAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string resourceLocation, 
			string accountName, 
			Api apiType)
		{

			DatabaseAccountCreateUpdateParameters createUpdateParameters = new DatabaseAccountCreateUpdateParameters
			{
				Location = resourceLocation, //region where account metadata for the ARM-tracked databaseAccount resource is created
				Locations = new List<Location> //regions where data is stored/replicated
				{
					new Location { LocationName = "West US", FailoverPriority = 0, IsZoneRedundant = false },
					new Location { LocationName = "East US", FailoverPriority = 1, IsZoneRedundant = false }
				},
				ConsistencyPolicy = new ConsistencyPolicy { DefaultConsistencyLevel = DefaultConsistencyLevel.Session },
				EnableAutomaticFailover = true, //also referred to as system-managed failover
				DisableKeyBasedMetadataWriteAccess = false,  //setting this to true blocks SDK clients from changing any control plane resources
				EnableFreeTier = false,
				EnableMultipleWriteLocations = false,
				//EnableAnalyticalStorage = false,
				//AnalyticalStorageConfiguration = new AnalyticalStorageConfiguration { SchemaType = "WellDefined" }, //Can be WellDefined or FullFidelity
				//KeyVaultKeyUri = string.Empty, //KeyVault URI for customer-managed encryption keys
				//DefaultIdentity = "SystemAssignIdentity", //Identity used to access keys in KeyVault
				//IpRules = new List<IpAddressOrRange>() { },
				//IsVirtualNetworkFilterEnabled = false,
				//VirtualNetworkRules = new List<VirtualNetworkRule>() { },
				//NetworkAclBypass = NetworkAclBypass.AzureServices,
				//NetworkAclBypassResourceIds = new List<string>() { },
				//PublicNetworkAccess = "enabled",
				//Cors = new List<CorsPolicy>() { },
				//DisableLocalAuth = false, //when set to true, SDK's can only use MSI or AAD to authenticate rather than master keys
				CreateMode = "Default", //Default is regular account creation, or Restore from backup
				Tags = new Dictionary<string, string>() { }
				
			};

            //Serverless Mode (Any resource provisions with throughput will throw an exception)
            //createUpdateParameters.Capabilities.Add(new Capability { Name = "EnableServerless" });

            SetApi(createUpdateParameters, apiType);

			return await cosmosClient.DatabaseAccounts.CreateOrUpdateAsync(resourceGroupName, accountName, createUpdateParameters);
		}

        public async Task<List<string>> ListAccountsAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName)
        {

	        var cosmosAccounts = await cosmosClient.DatabaseAccounts.ListByResourceGroupAsync(resourceGroupName);

			List<string> accountNames = new List<string>();

			foreach(var account in cosmosAccounts)
			{
				accountNames.Add(account.Name);
			}
			return accountNames;
        }

        public async Task<DatabaseAccountGetResults> GetAccountAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
        {
			DatabaseAccountGetResults databaseAccount = await cosmosClient.DatabaseAccounts.GetAsync(resourceGroupName, accountName);

			Console.WriteLine($"Resource Id: {databaseAccount.Id}");
			
			Console.WriteLine($"Name: {databaseAccount.Name}");
            
			Console.WriteLine($"Api: {GetApi(databaseAccount)}");
            
			Console.WriteLine($"Default Consistency Level: {databaseAccount.ConsistencyPolicy.DefaultConsistencyLevel}");
            
			Console.WriteLine($"Connection Endpoint: {databaseAccount.DocumentEndpoint}");

            foreach (string capability in GetCapabilities(databaseAccount))
			{
				Console.WriteLine($"Capability: {capability}");
			}

			if(databaseAccount.Kind == "MongoDB" )
				Console.WriteLine($"Server Version: {databaseAccount.ApiProperties.ServerVersion}");
			
            bool isMultiMaster = false;
            if (databaseAccount.EnableMultipleWriteLocations.GetValueOrDefault())
            {
                isMultiMaster = true;
                Console.WriteLine("Multi-Region Writes Enabled: true");
            }
            Console.WriteLine($"Free Tier: {databaseAccount.EnableFreeTier.GetValueOrDefault()}");
            Console.WriteLine("\nList Replicated Regions for Cosmos DB Account\n------------------------------------");
			foreach (Location location in databaseAccount.Locations)
			{
				Console.WriteLine($"Location Id: {location.Id}");
				Console.WriteLine($"Location Region: {location.LocationName}");
				Console.WriteLine($"Location Failover Priority: {location.FailoverPriority}");
				if (location.FailoverPriority.GetValueOrDefault() == 0 && !isMultiMaster)
					Console.WriteLine("Is Write Region: true");
				Console.WriteLine($"Is Availability Zone: {location.IsZoneRedundant}");
				Console.WriteLine("------------------------------------");
			}

            Console.WriteLine($"Enable Analytical Storage: {databaseAccount.EnableAnalyticalStorage.GetValueOrDefault().ToString()}");
			
			Console.WriteLine($"Backup Policy: {databaseAccount.BackupPolicy.ToString()}");

            Console.WriteLine($"Enable System-Managed Failover: {databaseAccount.EnableAutomaticFailover.GetValueOrDefault()}");

			Console.WriteLine($"Disable DataPlane SDK access to Control Plane: {databaseAccount.DisableKeyBasedMetadataWriteAccess.GetValueOrDefault()}");

            Console.WriteLine($"Default Identity: {databaseAccount.DefaultIdentity}");

            if (databaseAccount.KeyVaultKeyUri == null)
                Console.WriteLine($"Encryption using Service-Managed Key");
            else
				Console.WriteLine($"Encryption using Cusotmer-Managed Key. KeyVault Uri: {databaseAccount.KeyVaultKeyUri}");

			if (databaseAccount.IpRules.Count > 0)
			{
				Console.WriteLine("\nIP Rules\n------------------------------------");
				foreach (IpAddressOrRange ipAddress in databaseAccount.IpRules)
				{
					Console.WriteLine($"\tIP Address or Range: {ipAddress.IpAddressOrRangeProperty}");
				}
			}

			if (databaseAccount.IsVirtualNetworkFilterEnabled.GetValueOrDefault())
			{
				Console.WriteLine("\nVirtual Network Rules\n------------------------------------");
				foreach (VirtualNetworkRule virtualNetworkRule in databaseAccount.VirtualNetworkRules)
				{
					Console.WriteLine($"\tVirtual Network Rule: {virtualNetworkRule.Id}");
				}
			}

			if(databaseAccount.PrivateEndpointConnections != null)
			{
				Console.WriteLine("\nPrivate Endpoint Connections\n------------------------------------");
				foreach(PrivateEndpointConnection privateEndpoint in databaseAccount.PrivateEndpointConnections)
				{
					Console.WriteLine($"\tName: {privateEndpoint.Name}");
					Console.WriteLine($"\tType: {privateEndpoint.Type}");
					Console.WriteLine($"\tConnection Status: {privateEndpoint.PrivateLinkServiceConnectionState.Status}");
					Console.WriteLine($"\tActions Required: {privateEndpoint.PrivateLinkServiceConnectionState.ActionsRequired}");
				}
			}

			if(databaseAccount.Tags.Count > 0)
            {
				Console.WriteLine("\nTags\n------------------------------------");
				foreach(var tag in databaseAccount.Tags)
                {
					Console.WriteLine($"Tag Key: {tag.Key} \tTag Value:{tag.Value}");
				}
			}

			return databaseAccount;
		}

		public async Task<DatabaseAccountListKeysResult> ListKeysAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{

			DatabaseAccountListKeysResult keys = await cosmosClient.DatabaseAccounts.ListKeysAsync(resourceGroupName, accountName);

			Console.WriteLine($"Primary Key: {keys.PrimaryMasterKey}");
			Console.WriteLine($"Primary Readonly Key: {keys.PrimaryReadonlyMasterKey}");
			Console.WriteLine($"Secondary Key: {keys.SecondaryMasterKey}");
			Console.WriteLine($"Secondary Readonly Key: {keys.SecondaryReadonlyMasterKey}");

			Console.WriteLine("Press any key to continue.");
			Console.ReadKey();

			return keys;
		}

		public async Task<DatabaseAccountGetResults> UpdateAccountAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{
			//DatabaseAccount supports patch operations so simply pass in the value for the updated properties and call update.
			//Note that you cannot update Locations and other properties simultaneously. Doing so will throw an exception.

			Console.WriteLine("Change the default consistency policy for the account to eventual");

            DatabaseAccountUpdateParameters databaseAccountUpdateParameters = new DatabaseAccountUpdateParameters
			{
				ConsistencyPolicy = new ConsistencyPolicy
				{
					DefaultConsistencyLevel = DefaultConsistencyLevel.Eventual
				}
			};

			return await cosmosClient.DatabaseAccounts.UpdateAsync(resourceGroupName, accountName, databaseAccountUpdateParameters);
		}

		public async Task<List<Location>> AddRegionAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{
			List<Location> locations = new List<Location>
			{
				new Location { LocationName = "West US 2", FailoverPriority = 0, IsZoneRedundant = false },
				new Location { LocationName = "East US 2", FailoverPriority = 1, IsZoneRedundant = false },
				new Location { LocationName = "South Central US", FailoverPriority = 2, IsZoneRedundant = false } //add a new region
			};

			DatabaseAccountUpdateParameters databaseAccountUpdateParameters = new DatabaseAccountUpdateParameters
			{
				Locations = locations
			};

			await cosmosClient.DatabaseAccounts.UpdateAsync(resourceGroupName, accountName, databaseAccountUpdateParameters);

			return locations;
		}

		public async Task<FailoverPolicies> ChangeFailoverPriority(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{
			//In this operation we are only swapping the last two read regions. This can be done with no downtime.
			FailoverPolicies failoverPolicies = new FailoverPolicies
			{
				FailoverPoliciesProperty = new List<FailoverPolicy>
				{
					new FailoverPolicy { LocationName = "West US 2", FailoverPriority = 0 },
					new FailoverPolicy { LocationName = "South Central US", FailoverPriority = 1 }, //Swap these two regions
					new FailoverPolicy { LocationName = "East US 2", FailoverPriority = 2 } //Swap these two regions
				}
			};

			await cosmosClient.DatabaseAccounts.FailoverPriorityChangeAsync(resourceGroupName, accountName, failoverPolicies);

			return failoverPolicies;
		}

		public async Task<FailoverPolicies> InitiateFailover(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{
			//Initiate a failover by updating the first region (Failover Priority = 0) to a secondary region
			//Before starting this operation, make sure the Automatic Failover = false
			//After the failover completes, set the account back to Automatic Failover

			//Turn off Automatic Failover
			await cosmosClient.DatabaseAccounts.UpdateAsync(resourceGroupName, accountName, new DatabaseAccountUpdateParameters { EnableAutomaticFailover = false });

			//Change Write region to East US 2 from West US 2
			FailoverPolicies failoverPolicies = new FailoverPolicies
			{
				FailoverPoliciesProperty = new List<FailoverPolicy>
				{
					new FailoverPolicy { LocationName = "East US 2", FailoverPriority = 0 },
					new FailoverPolicy { LocationName = "South Central US", FailoverPriority = 1 },
					new FailoverPolicy { LocationName = "West US 2", FailoverPriority = 2 }
				}
			};

			await cosmosClient.DatabaseAccounts.FailoverPriorityChangeAsync(resourceGroupName, accountName, failoverPolicies);

			//Turn on Automatic Failover
			await cosmosClient.DatabaseAccounts.UpdateAsync(resourceGroupName, accountName, new DatabaseAccountUpdateParameters { EnableAutomaticFailover = true });

			return failoverPolicies;
		}

		private void SetApi(DatabaseAccountCreateUpdateParameters createUpdateParameters, Api apiType)
		{

            //Add to any previously added capabilities
            IList<Capability> capabilities = createUpdateParameters.Capabilities;
			
			//If null then create a new array
			if(capabilities == null)
				capabilities = new List<Capability>();

            switch (apiType)
			{
				case Api.Sql:
					createUpdateParameters.Kind = "GlobalDocumentDB";
					break;
				case Api.MongoDB:
					createUpdateParameters.Kind = "MongoDB";
					createUpdateParameters.ApiProperties = new ApiProperties { ServerVersion = "4.2" }; //Values: 3.2, 3.6, 4.0, 4.2
					capabilities.Add(new Capability { Name = "DisableRateLimitingResponses" }); //Enable server-side retries
					break;
				case Api.Cassandra:
                    capabilities.Add( new Capability { Name = "EnableCassandra" });
					break;
				case Api.Gremlin:
					capabilities.Add( new Capability { Name = "EnableGremlin" });
					break;
				case Api.Table:
					capabilities.Add( new Capability { Name = "EnableTable" });
					break;
			}

			createUpdateParameters.Capabilities = capabilities;
		}
		private string GetApi(DatabaseAccountGetResults cosmosAccount)
		{
			
			if(cosmosAccount.Kind == "MongoDB")
			{
				return "MongoDB";
			}
			else
			{
				if (cosmosAccount.Capabilities.Count > 0)
				{
					foreach (Capability capability in cosmosAccount.Capabilities)
					{
						switch (capability.Name)
						{
							case "EnableGremlin":
								return "Gremlin";
							case "EnableCassandra":
								return "Cassandra";
							case "EnableTable":
								return "Table";
						}
					}
					return "Sql";
				}
				else
				{
					return "Sql";
				}
			}
		}
		private List<string> GetCapabilities(DatabaseAccountGetResults cosmosAccount)
		{
			//return any non API capabilities for account, GetApi() returns those.
			List<string> capabilities = new List<string>();
			if (cosmosAccount.Capabilities.Count > 0)
			{
				foreach (Capability capability in cosmosAccount.Capabilities)
				{
					switch (capability.Name)
					{
						case "EnableGremlin":
							break;
						case "EnableCassandra":
							break;
						case "EnableTable":
							break;
						default:
							capabilities.Add(capability.Name);
							break;
					}
				}	
			}
			return capabilities;
		}
	}
}
