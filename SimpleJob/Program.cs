using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SimpleJob
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            int result = 0;

            //Is App Insights configured?
            IConfiguration config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            string telemetaryKey = config["AI_KEY"];
            if (string.IsNullOrEmpty(telemetaryKey))
            {
                return -1;
            }

            TelemetryConfiguration.Active.InstrumentationKey = telemetaryKey;
            TelemetryClient telemetryClient = new TelemetryClient();

            string jobStatus = string.Format("{0}: Running on {1} from machine {2}", DateTime.Now.ToString(), Environment.OSVersion, Environment.MachineName);
            telemetryClient.TrackTrace(jobStatus);

            result = await ReadFromQueue(config, telemetryClient);

            telemetryClient.Flush();

            return result;
        }

        private static async Task<int> ReadFromQueue(IConfiguration config, TelemetryClient telemetryClient)
        {
            int result = 0;
            string storageConnectionString = config["STORAGE_CONNECTION"];
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference("input");
                CloudQueueMessage queueMessage = await queue.GetMessageAsync();

                string storageMessage = queueMessage.AsString;
                telemetryClient.TrackTrace(storageMessage);

                await queue.DeleteMessageAsync(queueMessage);
                result = 1;
            }
            catch
            {
                Console.WriteLine("Unable to read storage queue: {0}", storageConnectionString);
            }

            return result;
        }
    }
}
