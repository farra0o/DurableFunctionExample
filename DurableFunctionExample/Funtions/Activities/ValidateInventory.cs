using DurableFunctionExample.DBContext;
using DurableFunctionExample.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DurableFunctionExample.Funtions.Activities;

public class ValidateInventory
{
    
    private readonly ILogger<ValidateInventory> _logger;
    private readonly OrderDbContext _db; 

    public ValidateInventory(ILogger<ValidateInventory> logger, OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function(nameof(ValidateInventory))]
    public async Task<bool> Run([ActivityTrigger] RequestDTO order)
    {
        _logger.LogInformation("Validando inventario en BD para {OrderId}", order.Items);

        try
        {
            foreach (var item in order.Items)
            {
                var product = await _db.Items
                    .FindAsync(item.ItemId); // Busca por ProductId en tu tabla Inventory

                if (product == null)
                {
                    _logger.LogWarning("Producto {ProductId} no existe en inventario", item.ItemId);
                    return false;
                }

                if (product.ItemStock < item.Quantity)
                {
                    _logger.LogWarning("Stock insuficiente para producto {ProductId}. Necesari{Needed},Disponible: {Available}",
                    item.ItemId, item.Quantity, product.ItemStock);
                    return false;
                }
            }
        }
        catch (Exception e) { _logger.LogError($"Error", e); }

        return true;
    }
}

 