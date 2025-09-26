using DurableFunctionExample.DBContext;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionExample.Funtions.Activities;

public class UpdateStock
{
    private readonly ILogger<UpdateStock> _logger;
    private readonly OrderDbContext _db;
    public UpdateStock(ILogger<UpdateStock> logger , OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function(nameof(UpdateStock))]
    public async Task<bool> Run([ActivityTrigger] Order order)
    {
        _logger.LogInformation("Actualizando stock ");

        foreach (var item in order.Items)
        {
            _logger.LogInformation("ItemId: {ItemId}, Cantidad: {Cantidad}", item.ItemOrderId, item.Cantidad);

            // Buscar el producto en la base de datos
            // al buscar por FindAsync, Hay track de la actividad y EF Core sabe que se va a actualizar
            var ItemBD = await _db.Items.FindAsync(item.ItemOrderId);
            if (ItemBD == null)
            {
                _logger.LogWarning("Producto {ItemId} no encontrado en la base de datos", item.ItemOrderId);
                continue;
            }

            ItemBD.ItemStock -= item.Cantidad;
            _logger.LogInformation("Nuevo stock para ItemId {ItemId}: {Stock}", ItemBD.ItemId, ItemBD.ItemStock);
        }
        
        // Guardar cambios en la BD
        await _db.SaveChangesAsync();

        return true;
    }
}