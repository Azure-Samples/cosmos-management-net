﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace cosmos_management_fluent
{
    class Gremlin
    {
		public async Task<ICosmosDBAccount> CreateGraphAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName, 
			string databaseName, 
			string graphName)
		{
			ICosmosDBAccount account = await azure.CosmosDBAccounts.Define(accountName)
				.WithRegion(Region.USWest2)
				.WithExistingResourceGroup(resourceGroupName)
				.WithDataModelGremlin()
				.WithSessionConsistency()
				.WithWriteReplication(Region.USWest2)
				.WithReadReplication(Region.USEast2)
				//.WithAutomaticFailover(true)  //AutomaticFailover not yet implemented
				.DefineNewGremlinDatabase(databaseName)
					.DefineNewGremlinGraph(graphName)
					.WithThroughput(400)
					.WithPartitionKey(paths: new List<string>() { "/myPartitionKey" }, kind: PartitionKind.Hash, version: null)
					.DefineIndexingPolicy()
						.WithAutomatic(true)
						.WithIndexingMode(IndexingMode.Consistent)
						.WithIncludedPath(new IncludedPath(path: "/*"))
						.WithExcludedPath(new ExcludedPath(path: "/myPathToNotIndex/*"))
						.WithCompositeIndexes(
							 new List<IList<CompositePath>>
								{
								new List<CompositePath>
								{
									new CompositePath { Path = "/myOrderByPath1", Order = CompositePathSortOrder.Ascending },
									new CompositePath { Path = "/myOrderByPath2", Order = CompositePathSortOrder.Descending }
								}
							})
							.Attach()
						.Attach()
					.Attach()
				.CreateAsync();

			return account;
		}

		public async Task<ThroughputSettingsGetPropertiesResource> GetGraphThroughputSettingsAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName, 
			string databaseName, 
			string graphName)
		{
			ThroughputSettingsGetPropertiesResource throughput = await azure.CosmosDBAccounts
				.GetByResourceGroup(resourceGroupName, accountName)
				.GetGremlinDatabase(databaseName)
				.GetGremlinGraph(graphName)
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

		public async Task<int> UpdateGraphThroughputAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName, 
			string databaseName, 
			string graphName, 
			int throughput)
		{
			var throughputSettings = await GetGraphThroughputSettingsAsync(azure, resourceGroupName, accountName, databaseName, graphName);

			if (throughputSettings.OfferReplacePending == "true")
				Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

			int minThroughput = Convert.ToInt32(throughputSettings.MinimumThroughput);

			//Check if passed throughput is less than minimum allowable
			if (throughput < minThroughput)
			{
				Console.WriteLine($"Throughput value passed: {throughput} is below Minimum allowable throughput {minThroughput}. Setting to minimum throughput.");
				throughput = minThroughput;
			}

			await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
			.UpdateGremlinDatabase(databaseName)
				.UpdateGremlinGraph(graphName)
					.WithThroughput(throughput)
					.Parent()
				.Parent()
			.ApplyAsync();

			return throughput;
		}

		public async Task UpdateGraphAsync(
			IAzure azure, 
			string resourceGroupName, 
			string accountName, 
			string databaseName, 
			string containerName)
		{

			await azure.CosmosDBAccounts.GetByResourceGroup(resourceGroupName, accountName).Update()
				.UpdateGremlinDatabase(databaseName)
					.UpdateGremlinGraph(containerName)
						.UpdateIndexingPolicy()
							.WithoutExcludedPath("/myPathNotToIndex/*") //add back to index
							.Parent()
						.Parent()
					.Parent()
				.ApplyAsync();
		}
	}
}
