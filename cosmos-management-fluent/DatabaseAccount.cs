﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Network.Fluent;

namespace cosmos_management_fluent
{
    class DatabaseAccount
    {

		public async Task<ICosmosDBAccount> CreateDatabaseAccountSqlAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			ICosmosDBAccount account = await azure.CosmosDBAccounts.Define(accountName)
				.WithRegion(Region.USWest2)
				.WithExistingResourceGroup(resourceGroupName)
				.WithDataModelSql()
				.WithSessionConsistency()
				.WithWriteReplication(Region.USWest2, false)
				.WithReadReplication(Region.USEast2, false)
				.WithMultipleWriteLocationsEnabled(false)
				.WithDisableKeyBaseMetadataWriteAccess(false)
				.WithVirtualNetworkFilterEnabled(false)
				.WithIpRangeFilter("")
				.WithKeyVault("")
				.WithVirtualNetworkRule("networkRuleResourceId", "subnetName", ignoreMissingVNetServiceEndpoint: true)
				.WithAutomaticFailoverEnabled(true)
				.CreateAsync();

			return account;
		}

		public async Task<List<string>> ListDatabaseAccountsAsync(
			IAzure azure, 
			string resourceGroupName)
		{
			var cosmosAccounts = await azure.CosmosDBAccounts.ListByResourceGroupAsync(resourceGroupName);

			List<string> accountNames = new List<string>();

			foreach (var account in cosmosAccounts)
			{
				accountNames.Add(account.Name);
			}
			return accountNames;
		}

		public async Task<ICosmosDBAccount> GetAccountAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			ICosmosDBAccount account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			Console.WriteLine($"Resource Id: {account.Id}");
			Console.WriteLine($"Name: {account.Name}");
			Console.WriteLine($"Api: {GetApi(account)}");

			foreach (string capability in GetCapabilities(account))
			{
				Console.WriteLine($"Capability: {capability}");
			}

			Console.WriteLine($"Default Consistency Level: {account.ConsistencyPolicy.DefaultConsistencyLevel.ToString()}");
			Console.WriteLine($"Connection Endpoint: {account.DocumentEndpoint}");

			Console.WriteLine($"Automatic Failover Enabled: {account.AutomaticFailoverEnabled.ToString()}");
			Console.WriteLine($"Key Based Metadata Write Access Disabled: {account.KeyBasedMetadataWriteAccessDisabled.ToString()}");
			Console.WriteLine($"Customer Managed Encryption KeyVault Uri: {account.KeyVaultUri}");

			bool isMultiMaster = false;
			if (account.MultipleWriteLocationsEnabled.GetValueOrDefault())
			{
				isMultiMaster = true;
				Console.WriteLine("Multi-Master Enabled: true");
			}

			Console.WriteLine("\nList Replica Regions for Cosmos Account\n------------------------------------");
			foreach (Location location in account.ReadableReplications)
			{
				Console.WriteLine($"Location Id: {location.Id}");
				Console.WriteLine($"Location Region: {location.LocationName}");
				Console.WriteLine($"Location Failover Priority: {location.FailoverPriority}");
				if (location.FailoverPriority.GetValueOrDefault() == 0 && !isMultiMaster)
					Console.WriteLine("Is Write Region: true");
				Console.WriteLine($"Is Availability Zone: {location.IsZoneRedundant}");
				Console.WriteLine("------------------------------------");
			}

			if (account.IPRangeFilter.Length > 0)
				Console.WriteLine($"IP Range Filter: {account.IPRangeFilter}");

			//VirtualNetworkFilterEnabled is not yet implemented
			//if (account.IsVirtualNetworkFilterEnabled.GetValueOrDefault())
			{
				foreach (VirtualNetworkRule virtualNetworkRule in account.VirtualNetworkRules)
				{
					Console.WriteLine($"Virtual Network Rule: {virtualNetworkRule.Id}");
					Console.WriteLine($"Ignore Missing VNet Service Endpoint: {virtualNetworkRule.IgnoreMissingVNetServiceEndpoint}");
				}
			}

			return account;
		}

		public async Task ListKeysAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			var keys = await azure.CosmosDBAccounts.ListKeysAsync(resourceGroupName, accountName);

			Console.WriteLine($"Primary Key: {keys.PrimaryMasterKey}");
			Console.WriteLine($"Primary Readonly Key: {keys.PrimaryReadonlyMasterKey}");
			Console.WriteLine($"Secondary Key: {keys.SecondaryMasterKey}");
			Console.WriteLine($"Secondary Readonly Key: {keys.SecondaryReadonlyMasterKey}");
		}

		public async Task AddRegionAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{

			var account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			await account
				.Update()
				.WithReadReplication(region:Region.USSouthCentral, isZoneRedundant:false) //Add new region
				.ApplyAsync();

		}

		public async Task RemoveRegionAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			var account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			await account
				.Update()
				.WithoutReadReplication(Region.USSouthCentral) //Remove region
				.ApplyAsync();
		}

		public async Task ChangeFailoverPriorityAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			//In this operation we are only swapping the last two read regions. This can be done with no downtime.
			//Assume current regions are West US 2 = 0, East US 2 = 1, South Central US = 3

			var account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			await account.Update()
				.WithoutAllReplications()
				.WithWriteReplication(region: Region.USWest2)
				.WithReadReplication(region: Region.USSouthCentral)
				.WithReadReplication(region: Region.USEast2)
				.ApplyAsync();

		}

		public async Task InitiateFailoverAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			//Initiate a failover by updating the region with FailoverPriority = 0. This operation incurs downtime for write region.
			//Assume current regions are West US 2 = 0, South Central US = 1, East US 2 = 3

			var account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			await account.Update()
				.WithoutAllReplications()
				.WithWriteReplication(region: Region.USEast2)
				.WithReadReplication(region: Region.USSouthCentral)
				.WithReadReplication(region: Region.USWest2)
				.ApplyAsync();

		}
			
		public async Task UpdateAccountAddVirtualNetworkAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName, 
			INetwork virtualNetwork)
		{
			var account = await azure.CosmosDBAccounts.GetByResourceGroupAsync(resourceGroupName, accountName);

			//Update account with new virtual network and subnet
			await account.Update()
				.WithVirtualNetworkRule(virtualNetwork.Id, "subnet1")
				.ApplyAsync();
				
		}

		public async Task<INetwork> CreateVirtualNetworkAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{
			INetwork virtualNetwork = await azure.Networks.Define(accountName)
				.WithRegion(Region.USWest2)
				.WithNewResourceGroup(resourceGroupName)
				.WithAddressSpace("10.0.0.0/16")
				.DefineSubnet("subnet1")
					.WithAddressPrefix("10.0.1.0/24")
					.WithAccessFromService(ServiceEndpointType.MicrosoftAzureCosmosDB)
					.Attach()
				.DefineSubnet("subnet2")
					.WithAddressPrefix("10.0.2.0/24")
					.WithAccessFromService(ServiceEndpointType.MicrosoftAzureCosmosDB)
					.Attach()
				.CreateAsync();

			return virtualNetwork;
		}

		public async Task DeleteAccountAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName)
		{

			await azure.CosmosDBAccounts.DeleteByResourceGroupAsync(resourceGroupName, accountName);
		}

		private string GetApi(ICosmosDBAccount account)
		{

			if (account.Kind == "MongoDB")
			{
				return "MongoDB";
			}
			else
			{
				if (account.Capabilities.Count > 0)
				{
					foreach (Capability capability in account.Capabilities)
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
		private List<string> GetCapabilities(ICosmosDBAccount account)
		{
			//return any non API capabilities for account
			List<string> capabilities = new List<string>();
			if (account.Capabilities.Count > 0)
			{
				foreach (Capability capability in account.Capabilities)
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
