using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunctionExample.Funtions.Orchestrators;

public static class Orchestrator
{
    [Function(nameof(Orchestrator))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(Orchestrator));
        logger.LogInformation("Orchstrator Intro");
        //01 Validar inventario
        //02 Crear Usuario
        //03 Crear Orden
        //04 validar pago
        //05 actualizar estado de la orden
        //05 ActualizarStock
        //06 Envviar json de la orden
        // http para Validar token enviar 202 y llamar a la funcion orquestadora


        return default;
    }
}
    