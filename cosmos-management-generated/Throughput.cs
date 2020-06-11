using System;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    static class Throughput
    {

        static public CreateUpdateOptions Create (
            int? throughput, 
            bool? isAutoScale = false)
        {

            CreateUpdateOptions createUpdateOptions = new CreateUpdateOptions();

            if(throughput.HasValue) //if null then return empty options
            { 
                if (isAutoScale.Value)
                {
                    createUpdateOptions.AutoscaleSettings =  new AutoscaleSettings { MaxThroughput = throughput };
                }
                else
                {
                    createUpdateOptions.Throughput = throughput;
                }
            }
            return createUpdateOptions;
        }

        static public void Print(
            ThroughputSettingsGetPropertiesResource resource)
        {
            try
            {
                if (resource.AutoscaleSettings == null)
                {
                    Console.WriteLine($"Manual Provisioned Throughput: {resource.Throughput}");
                    Console.WriteLine($"Minimum Throughput: {resource.MinimumThroughput}");
                }
                else
                {
                    Console.WriteLine($"Max Autoscale Throughput: {resource.AutoscaleSettings.MaxThroughput}");
                    Console.WriteLine($"Target Max Autoscale Throughput: {resource.AutoscaleSettings.TargetMaxThroughput}");
                }
                Console.WriteLine($"Offer Replace Pending: {resource.OfferReplacePending}");
            }
            catch { }
        }

        static public ThroughputSettingsUpdateParameters Update (
            ThroughputSettingsGetPropertiesResource resource,
            int throughput,
            bool? autoScale = false)
        {

            ThroughputSettingsUpdateParameters throughputUpdate = new ThroughputSettingsUpdateParameters();
            ThroughputSettingsResource throughputSettingsResource = new ThroughputSettingsResource();

            if (resource.OfferReplacePending == "true")
                Console.WriteLine($"Throughput update in progress. This throughput replace will be applied after current one completes");

            if (!autoScale.GetValueOrDefault()) //manual throughput
            {
                int minThroughput = Convert.ToInt32(resource.MinimumThroughput);

                //Never set below min throughput or will generate exception
                if (minThroughput > throughput)
                    throughput = minThroughput;

                throughputSettingsResource.Throughput = throughput;
            }
            else //autoscale
            {
                throughputSettingsResource.AutoscaleSettings = new AutoscaleSettingsResource
                {
                    MaxThroughput = throughput
                };
                 
            }

            throughputUpdate.Resource = throughputSettingsResource;

            return throughputUpdate;
        }
    }
}
