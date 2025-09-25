using DurableFunctionExample.DBContext;
using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionExample.Funtions.Activities;

public class UpdateOrderStatatus
{
    private readonly ILogger<UpdateOrderStatatus> _logger;
    private readonly OrderDbContext _db;

    public UpdateOrderStatatus(ILogger<UpdateOrderStatatus> logger, OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function(nameof(UpdateOrderStatatus))]
    public async Task<Order> Run([ActivityTrigger] Order order)
    {
        _logger.LogInformation("Actualizando estado {OrderId}", order.OrderId);

        if (order.PaymentStatus == 1 )
        // Actualizar PaymentStatus: 1 = aprobado, 2 = rechazado
        order.Status = 1;
        else
            order.Status = 2;

        // Guardar cambios en la base de datos
        //_db.Orders.Update(order);
        //await _db.SaveChangesAsync();

        _logger.LogInformation("Orden {OrderId} actualizada con estado: PaymentStatus={PaymentStatus} , Referencia 1= Aceptada 2=Anulada", order.OrderId, order.Status);

        return order;
    }
}