using DurableFunctionExample.DTO;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DurableFunctionExample.Funtions.Http;

public class ResponseOrderFunction
{
    private readonly ILogger<ResponseOrderFunction> _logger;

    public ResponseOrderFunction(ILogger<ResponseOrderFunction> logger)
    {
        _logger = logger;
    }

    [Function("ResponseOrder_Http")]
    public async Task<HttpResponseData> Run( [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/status/{instanceId}")] HttpRequestData req,[DurableClient] DurableTaskClient client, string instanceId)
    {
        _logger.LogInformation("Consultando estado de la orden con ID {InstanceId}", instanceId);

        // Obtener el estado actual de la orquestación
        var orchestratorData = await client.GetInstanceAsync(instanceId);

        // Leer Output fuertemente tipado (TaskResponseDTO)
        TaskResponseDTO? outputDto = orchestratorData.ReadOutputAs<TaskResponseDTO>();

        // Construir objeto anónimo para el JSON
        var result = new
        {
            instanceId = orchestratorData.InstanceId,
            runtimeStatus = orchestratorData?.RuntimeStatus.ToString(),
            output = outputDto
        };

        // Crear respuesta HTTP
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);

        return response;
    }
}