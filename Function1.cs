using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctions.Playground
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(RaiseDurableEvent), null);
            while (true)
            {
                await context.WaitForExternalEvent("my-durable-event");
            }
        }

        [FunctionName("RaiseDurableEvent")]
        public static async Task RaiseDurableEvent([ActivityTrigger] IDurableActivityContext context, ILogger log, [DurableClient] IDurableOrchestrationClient client)
        {
            await client.RaiseEventAsync(context.InstanceId, "my-durable-event");
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}