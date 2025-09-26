using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunctionExample.Funtions.Orchestrators;

public static class Orchestrator
{
    [Function(nameof(Orchestrator))]
    public static async Task<TaskResponseDTO> Run(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(Orchestrator));
        logger.LogInformation("Orchstrator Intro");
        var requestDTO =context.GetInput<RequestDTO>();

        //01 Validar inventario
        bool inventoryOk = false;
        
        inventoryOk = await context.CallActivityAsync<bool>("ValidateInventory", requestDTO);
        logger.LogInformation($"Inventario validado: {inventoryOk}");
     
        //02 Crear Usuario y devolver UserRequestDTO
        UserWithRequestDTO userWRequest = null;
        userWRequest = await context.CallActivityAsync<UserWithRequestDTO>("CreateUser", requestDTO);

        //03 Crear Orden
        Order order = null;
        order = await context.CallActivityAsync<Order>("CreateOrder", userWRequest);
        //04 validar pago
        order = await context.CallActivityAsync<Order>("CheckPayment", order);
        //05 actualizar estado de la orden
        order = await context.CallActivityAsync<Order>("UpdateOrderStatus", order);
        //05 ActualizarStock
        bool InventoryUpdated = false;
        if (order.Status == 1)
        {
            InventoryUpdated = await context.CallActivityAsync<bool>("UpdateStock", order);
        }
        //06 Generar Respuesta de la orden
        var response = await context.CallActivityAsync<TaskResponseDTO>("ResponseOrder", order);

        return response;
    }
}
    