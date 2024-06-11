import azure.functions as func
import azure.durable_functions as df
from azure.durable_functions import DurableOrchestrationContext, DurableOrchestrationClient

myApp = df.DFApp(http_auth_level=func.AuthLevel.ANONYMOUS)

@myApp.route(route="orchestrators/{functionName}")
@myApp.durable_client_input(client_name="client")
async def http_start(req: func.HttpRequest, client: DurableOrchestrationClient):
    function_name = req.route_params.get('functionName')
    instance_id = await client.start_new(function_name)
    response = client.create_check_status_response(req, instance_id)
    return response

# Orchestrator
@myApp.orchestration_trigger(context_name="context")
def hello_orchestrator(context: DurableOrchestrationContext):
    yield context.call_activity("raise_my_durable_event", context.instance_id)
    while True:
        yield context.wait_for_external_event("my-durable-event")

# Activity
@myApp.activity_trigger(input_name="instance", activity="raise_my_durable_event")
@myApp.durable_client_input(client_name="client")
async def raise_my_durable_event(client: DurableOrchestrationClient, instance):
    await client.raise_event(instance, "my-durable-event")
