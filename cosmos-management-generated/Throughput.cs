using System;
using Microsoft.Azure.Management.CosmosDB.Models;

namespace cosmos_management_generated
{
    static class Throughput
    {

        static public CreateUpdateOptions Create (
            int throughput, 
            bool isAutoScale)
        {

            CreateUpdateOptions createUpdateOptions = new CreateUpdateOptions();

            if (isAutoScale == true)
                createUpdateOptions.AutoscaleSettings =  new AutoscaleSettings { MaxThroughput = throughput };
            else
                createUpdateOptions.Throughput = throughput;

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
            bool autoScale = false)
        {

            ThroughputSettingsUpdateParameters throughputUpdate = new ThroughputSettingsUpdateParameters();
            ThroughputSettingsResource throughputSettingsResource = new ThroughputSettingsResource();

            if (resource.OfferReplacePending == "true")
                Console.WriteLine($"Throughput update in progress. New throughput value will be applied after current one completes");

            if (autoScale == false) //manual throughput
            {
                int minThroughput = Convert.ToInt32(resource.MinimumThroughput);

                //Never set below min throughput or will generate exception
                if (throughput < minThroughput)
                { 
                    throughput = minThroughput;
                    Console.WriteLine($"Passed throughput value: {throughput} is below minimum throughput value: {minThroughput}. Setting throughput to minimum throughput amount");
                }
                throughputSettingsResource.Throughput = throughput;
            }
            else //autoscale
            {
                int minThroughput = Convert.ToInt32(resource.MinimumThroughput);

                //Max throughput must be 10X the minimum throughput value so scale down does not go below minimum throughput. Otherwise will generate exception.
                if (throughput < minThroughput * 10)
                {
                    throughput = minThroughput;
                    Console.WriteLine($"Passed throughput value: {throughput} is below minimum throughput value: {minThroughput} X 10 or {minThroughput*10}. Setting autoscale max throughput to {minThroughput*10}");
                }

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
