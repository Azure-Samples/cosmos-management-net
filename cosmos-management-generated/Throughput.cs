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
            bool? isAutoScale = false)
        {

            CreateUpdateOptions createUpdateOptions = new CreateUpdateOptions();
            ProvisionedThroughputSettingsResource autoscaleThroughput;

            if (isAutoScale.Value)
            {
                autoscaleThroughput = new ProvisionedThroughputSettingsResource { MaxThroughput = throughput };

                createUpdateOptions.AdditionalProperties = new Dictionary<string, string>()
                {
                    { "ProvisionedThroughputSettings", autoscaleThroughput.ToString().Replace("\"", "\\\"") }
                };
            }
            else
            {
                createUpdateOptions.Throughput = throughput.ToString();
            }

            return createUpdateOptions;
        }

        static public void Get(
            ThroughputSettingsGetPropertiesResource resource)
        {
            try
            {
                ProvisionedThroughputSettingsResource autoscale = resource.ProvisionedThroughputSettings;

                if (autoscale == null)
                {
                    Console.WriteLine($"Manual Provisioned Throughput: {resource.Throughput}");
                    Console.WriteLine($"Minimum Throughput: {resource.MinimumThroughput}");
                    Console.WriteLine($"Offer Replace Pending: {resource.OfferReplacePending}");
                }
                else
                {
                    Console.WriteLine($"Max Autoscale Throughput: {autoscale.MaxThroughput}");
                }
            }
            catch { }
        }

        static public ThroughputSettingsUpdateParameters Update (
            ThroughputSettingsGetPropertiesResource resource,
            int throughput,
            bool? autoScale = false)
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
            }

            return throughputUpdate;
        }
    }
}
