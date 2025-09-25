using DurableFunctionExample.Funtions.Orchestrators;
using DurableFunctionExample.Models;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;

using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;


namespace DurableFunctionExample.Funtions.Http;

public static class OrderFunctions
{
    [FunctionName("OrderHttpStart")]
    public static async Task<HttpResponseData> OrderHttpStart(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req,
    [Microsoft.Azure.Functions.Worker.DurableClient] DurableTaskClient client, TaskOrchestrationContext OrderContext)
    {
        ILogger _logger = OrderContext.CreateReplaySafeLogger(nameof(OrderFunctions));
        // Leer body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var order = JsonConvert.DeserializeObject<Order>(requestBody);

        // ------------------ AUTENTICACIÓN ------------------
        // Opción A: Easy Auth (App Service)
        if (req.Headers.TryGetValues("X-MS-CLIENT-PRINCIPAL", out var _))
        {
            _logger.LogInformation("Easy Auth presente → request autenticado por App Service.");
        }
        else
        {
            // Opción B: Validación manual del token de Google
            if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteStringAsync("Missing Authorization header");
                return unauthorized;
            }

            var authHeader = authHeaders.First();
            var idToken = authHeader.StartsWith("Bearer ") ? authHeader.Substring(7) : authHeader;

            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { "636723774412-jcg6n8jkr2m849i6500o4gnlkq6ji4e9.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
                _logger.LogInformation($"Token válido para usuario: {payload.Email}");
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning($"Token inválido: {ex.Message}");
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteStringAsync("Invalid Google token");
                return unauthorized;
            }
        }

        // ------------------ ORQUESTACIÓN ------------------
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("OrderOrchestrator", order);
        _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        // Retornar 202 Accepted con URIs de polling (statusQueryGetUri, etc.)
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}