using DurableFunctionExample.DTO;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using System.Net;

namespace DurableFunctionExample.Funtions.Http;

public class OrderFunctions
{
    private readonly ILogger<OrderFunctions> _logger;

    public OrderFunctions(ILogger<OrderFunctions> logger)
    {
        _logger = logger;
    }
    //Si no se usa como String no se reconoce en el compilado como HttpTrigger
    [Function("CreateOrder_HttpStart")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post","OPTIONS", Route = "orders")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Check Json");
        //preflight OPTIONS request para contrarrestar problemas de CORS
        if (req.Method == "OPTIONS")
        {
            var optionsResponse = req.CreateResponse(HttpStatusCode.OK);
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            optionsResponse.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            optionsResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return optionsResponse;
        }

        // Leer body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
       
        var orderRequest= JsonConvert.DeserializeObject<RequestDTO>(requestBody);
       
        _logger.LogInformation("Check Token");
        // ------------------ AUTENTICACIÓN ------------------
        // Opción A: Easy Auth (App Service)
        if (req.Headers.TryGetValues("X-MS-CLIENT-PRINCIPAL", out var _))
        {
            _logger.LogInformation("Easy Auth presente → request autenticado por App Service.");
        }
        else
        {
            // Opción B: Validación manual del token de Google LOCAL 
            _logger.LogInformation("Local Validation");
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
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("OrderOrchestrator", orderRequest);
        _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        // Retornar 202 Accepted con URIs de polling (statusQueryGetUri, etc.)
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}