using Microsoft.Azure.Management.CosmosDB.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace cosmos_management_generated
{
    static class Throughput
    {

        static public CreateUpdateOptions Create (
            int throughput, 
            bool? isAutoScale = false, 
            bool? isAutoUpgrade = false, 
            int? incrementPercent = null)
        {

            CreateUpdateOptions createUpdateOptions = new CreateUpdateOptions();
            ProvisionedThroughputSettingsResource autoscaleThroughput;

            if (!isAutoScale.Value)
            {
                createUpdateOptions.Throughput = throughput.ToString();
            }
            else
            {
                if (!isAutoUpgrade.Value)
                {
                    autoscaleThroughput = new ProvisionedThroughputSettingsResource { MaxThroughput = throughput };
                }    
                else
                {
                    autoscaleThroughput = new ProvisionedThroughputSettingsResource
                    {
                        MaxThroughput = throughput,
                        AutoUpgradePolicy = new AutoUpgradePolicyResource
                        {
                            ThroughputPolicy = new ThroughputPolicyResource
                            {
                                IsEnabled = true,
                                IncrementPercent = incrementPercent
                            }
                        }
                    };
                }   

                createUpdateOptions.AdditionalProperties = new Dictionary<string, string>()
                {
                    { "ProvisionedThroughputSettings", autoscaleThroughput.ToString().Replace("\"", "\\\"") }
                };
            }

            return createUpdateOptions;
        }

        static public void Get(
            ThroughputSettingsGetPropertiesResource resource)
        {
            try
            {
                if (resource.Throughput.HasValue)
                {
                    Console.WriteLine($"Manual Provisioned Throughput: {resource.Throughput}");
                    Console.WriteLine($"Minimum Throughput: {resource.MinimumThroughput}");
                    Console.WriteLine($"Offer Replace Pending: {resource.OfferReplacePending}");
                }
                else
                {
                    ProvisionedThroughputSettingsResource autoscale = resource.ProvisionedThroughputSettings;

                    Console.WriteLine($"Max Autoscale Throughput: {autoscale.MaxThroughput}");

                    if (autoscale.AutoUpgradePolicy.ThroughputPolicy.IsEnabled.GetValueOrDefault())
                        Console.WriteLine($"Auto Upgrade Increment Percentage: {autoscale.AutoUpgradePolicy.ThroughputPolicy.IncrementPercent.Value}");
                }
            }
            catch { }
        }

        static public ThroughputSettingsUpdateParameters Update (
            ThroughputSettingsGetPropertiesResource resource,
            int throughput,
            bool? autoScale = false,
            bool? autoUpgrade = false,
            int? incrementPercent = null)
        {

            ThroughputSettingsUpdateParameters throughputUpdate = new ThroughputSettingsUpdateParameters();

            if (resource.OfferReplacePending == "true")
                Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

            if (!autoScale.GetValueOrDefault()) //manual throughput
            { 
                int minThroughput = Convert.ToInt32(resource.MinimumThroughput);

                //Never set below min throughput or will generate exception
                if (minThroughput > throughput)
                    throughput = minThroughput;

                throughputUpdate.Resource.Throughput = throughput;
            }
            else //autoscale
            {
                throughputUpdate.Resource.ProvisionedThroughputSettings.MaxThroughput = throughput;

                if(autoUpgrade.GetValueOrDefault())
                {
                    throughputUpdate.Resource.ProvisionedThroughputSettings.AutoUpgradePolicy.ThroughputPolicy.IsEnabled = true;
                    throughputUpdate.Resource.ProvisionedThroughputSettings.AutoUpgradePolicy.ThroughputPolicy.IncrementPercent = incrementPercent.Value;
                }
            }

            return throughputUpdate;
        }
    }
}
