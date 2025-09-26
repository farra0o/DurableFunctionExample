using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionExample.Funtions.Activities
{
    public class ResponseOrder
    {
        private readonly ILogger<ValidateInventory> _logger;
        public ResponseOrder(ILogger<ValidateInventory> logger)
        {
            _logger = logger;
        }
        [Function(nameof(ResponseOrder))]
        public async Task<TaskResponseDTO> Run([ActivityTrigger] Order OrderResponse)
        {
            _logger.LogInformation("Generando respuesta de la orden {OrderId}", OrderResponse.OrderId);

            var response = new TaskResponseDTO
            {
                OrderId = OrderResponse.OrderId,
                Status = OrderResponse.Status,
                Message = OrderResponse.Status == 1 ? "Orden completada" : "Orden fallida",
                UserId = OrderResponse.UserId
            };
            _logger.LogInformation("Respuesta generada {response}", response.ToString());
            return response;

        }
    }
}
