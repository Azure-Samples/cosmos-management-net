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

		public async Task<DatabaseAccountGetResults> CreateAccountAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string resourceLocation, 
			string accountName, 
			Api apiType)
		{

			DatabaseAccountCreateUpdateParameters createUpdateParameters = new DatabaseAccountCreateUpdateParameters
			{
				Location = resourceLocation, //region where account will be created
				Locations = new List<Location>
				{
					new Location { LocationName = "West US 2", FailoverPriority = 0, IsZoneRedundant = false },
					new Location { LocationName = "East US 2", FailoverPriority = 1, IsZoneRedundant = false }
				},
				ConsistencyPolicy = new ConsistencyPolicy
				{ 
					DefaultConsistencyLevel = DefaultConsistencyLevel.Session 
				},
				EnableAutomaticFailover = true,
				DisableKeyBasedMetadataWriteAccess = false  //setting this to true blocks SDK clients using access keys from changing any control plane resources
			};

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
			Console.WriteLine($"Free Tier: {databaseAccount.EnableFreeTier.GetValueOrDefault()}");
			Console.WriteLine($"Api: {GetApi(databaseAccount)}");
			
			foreach(string capability in GetCapabilities(databaseAccount))
			{
				Console.WriteLine($"Capability: {capability}");
			}

			if(databaseAccount.ApiProperties.ServerVersion.Length > 0)
			{
				Console.WriteLine($"Server Version: {databaseAccount.ApiProperties.ServerVersion}");
			}
			
			Console.WriteLine($"Default Consistency Level: {databaseAccount.ConsistencyPolicy.DefaultConsistencyLevel}");
			Console.WriteLine($"Connection Endpoint: {databaseAccount.DocumentEndpoint}");

			bool isMultiMaster = false;
			if (databaseAccount.EnableMultipleWriteLocations.GetValueOrDefault())
			{
				isMultiMaster = true;
				Console.WriteLine("Multi-Master Enabled: true");
			}

			Console.WriteLine("\nList Region Replicas for Cosmos Account\n------------------------------------");
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

			Console.WriteLine($"Enable Automatic Failover: {databaseAccount.EnableAutomaticFailover.GetValueOrDefault()}");

			Console.WriteLine($"Control Plane locked to RBAC only: {databaseAccount.DisableKeyBasedMetadataWriteAccess.GetValueOrDefault()}");

			if (databaseAccount.KeyVaultKeyUri.Length > 0)
				Console.WriteLine($"KeyVault Uri: {databaseAccount.KeyVaultKeyUri}");

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

			if(databaseAccount.PrivateEndpointConnections.Count > 0)
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

			return keys;
		}

		public async Task<DatabaseAccountGetResults> UpdateAccountAsync(
			CosmosDBManagementClient cosmosClient, 
			string resourceGroupName, 
			string accountName)
		{
			//DatabaseAccount supports patch operations so simply pass in the value for the updated properties and call update.
			//Note that you cannot update Locations and other properties simultaneously. Doing so will throw an exception.

			//Change the default consistency policy for the account to eventual
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
			switch (apiType)
			{
				case Api.Sql:
					createUpdateParameters.Kind = "GlobalDocumentDB";
					break;
				case Api.MongoDB:
					createUpdateParameters.Kind = "MongoDB";
					ApiProperties apiProperties = new ApiProperties { ServerVersion = "3.6" };
					createUpdateParameters.ApiProperties = apiProperties;
					break;
				case Api.Cassandra:
					createUpdateParameters.Capabilities = new List<Capability> { new Capability { Name = "EnableCassandra" } };
					break;
				case Api.Gremlin:
					createUpdateParameters.Capabilities = new List<Capability> { new Capability { Name = "EnableGremlin" } };
					break;
				case Api.Table:
					createUpdateParameters.Capabilities = new List<Capability> { new Capability { Name = "EnableTable" } };
					break;
			}
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
			//return any non API capabilities for account
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
