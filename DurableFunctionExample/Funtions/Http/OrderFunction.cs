using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DurableFunctionExample.Funtions.Http;

public class OrderFunction
{// http para Validar token enviar 202 y llamar a la funcion orquestadora
    private readonly ILogger<OrderFunction> _logger;

    public OrderFunction(ILogger<OrderFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OrderFunction))]
    public async Task<HttpResponseData> OrderHttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "orders")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Iniziando Function OrderFunction");
        return default;
    }
}