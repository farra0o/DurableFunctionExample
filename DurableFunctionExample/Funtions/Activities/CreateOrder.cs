using DurableFunctionExample.DBContext;
using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DurableFunctionExample.Funtions.Activities;

public class CreateOrder
{
    private readonly ILogger<CreateOrder> _logger;
    private readonly OrderDbContext _db;

    public CreateOrder(ILogger<CreateOrder> logger, OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [Function(nameof(CreateOrder))]
    public async Task<Order> Run([ActivityTrigger] UserWithRequestDTO userWRequest)
    {
        _logger.LogInformation("Creando orden para el usuario {UserId}", userWRequest.UserId);
        try
        {
            // Obtener los IDs de los items del request
            var itemIds = userWRequest.Request.Items.Select(i => i.ItemId).ToList();

            // Consultar los items en la base de datos
            var itemsFromDb = await _db.Items
                .Where(i => itemIds.Contains(i.ItemId))
                .ToListAsync();

            if (itemsFromDb.Count != itemIds.Count)
                throw new InvalidOperationException("Algunos items no existen en la base de datos.");

            // Construir la lista de ItemOrder con datos completos
            var itemOrders = userWRequest.Request.Items.Select(i =>
            {
                var item = itemsFromDb.First(x => x.ItemId == i.ItemId);
                return new ItemOrder
                {
                    ItemOrderId = i.ItemId,
                    Cantidad = i.Quantity,
                    Item = new ItemInOrderDto
                    {
                        ItemName = item.ItemName,
                        Price = item.Price
                    }
                };
            }).ToList();
            //Calcular el total
            var total = itemOrders.Sum(io => io.Cantidad * io.Item.Price);
            // Crear la orden completa
            var orderResult = new Order
            {
                UserId = userWRequest.UserId,
                Status = 1,
                PaymentStatus = 0,
                Items = itemOrders,
                TotalPrice = total
            };
            _logger.LogInformation("Creando orden para user {UserId} con {ItemCount} items. Total: {Total}", orderResult.UserId, orderResult.Items.Count, orderResult.TotalPrice);
            _db.Orders.Add(orderResult);
            await _db.SaveChangesAsync();
            return orderResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la orden");
            throw;
        }
    }        
}