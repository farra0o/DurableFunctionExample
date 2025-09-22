using System.Net;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Apis.Auth;// GoogleJsonWebSignature
using DurableFunctionExample.Models; 

namespace DurableFunctionExample;

public class OrderFunctions
{
    private readonly ILogger<OrderFunctions> _logger;
    public OrderFunctions(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<OrderFunctions>();
    }

    [Function("OrderHttpStart")]
    public async Task<HttpResponseData> OrderHttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
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
                    Audience = new[] { "GOOGLE_OAUTH_CLIENT_ID.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
                _logger.LogInformation("Token válido para usuario:" + payload.Email);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning("Token inválido: "+  ex.Message);
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteStringAsync("Invalid Google token");
                return unauthorized;
            }
        }

        // ORQUESTACIÓN 
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("OrderOrchestrator", order);
        _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        // Retornar 202 Accepted 
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}