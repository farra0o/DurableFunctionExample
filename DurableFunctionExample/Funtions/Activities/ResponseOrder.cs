using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionExample.Funtions.Activities
{
    public class ResponseOrder
    {
        [Function(nameof(ResponseOrder))]
        public async Task<TaskResponseDTO> Run([ActivityTrigger] Order OrderResponse)
        {
            var response = new TaskResponseDTO
            {
                OrderId = OrderResponse.OrderId,
                Status = OrderResponse.Status,
                Message = OrderResponse.Status == 1 ? "Orden completada" : "Orden fallida",
                UserId = OrderResponse.UserId
            };
            return response;
        }
    }
}
