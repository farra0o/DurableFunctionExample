using DurableFunctionExample.DBContext;
using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionExample.Funtions.Activities;

public class CheckPayment
{
    private readonly ILogger<CheckPayment> _logger;
    private readonly OrderDbContext _db;

    public CheckPayment(ILogger<CheckPayment> logger, OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function(nameof(CheckPayment))]
    public async Task<Order> Run([ActivityTrigger] Order order)
    {
        _logger.LogInformation("Validando pago de la orden {OrderId}", order.OrderId);
        var orderBD = await _db.Orders.FindAsync(order.OrderId);
        if (orderBD == null)
        {
            _logger.LogWarning("Orden {OrderId} no encontrada", order.OrderId);
            throw new Exception($"Orden {order.OrderId} no encontrada");
        }
        // Generar random para 85% aprobado, 15% rechazado
        Random rand = new Random();
        bool isApproved = rand.Next(100) < 85;

        // Actualizar PaymentStatus: 1 = aprobado, 2 = rechazado
        orderBD.PaymentStatus = isApproved ? 1 : 2;


        // Guardar cambios en la base de datos
        
        await _db.SaveChangesAsync();

        _logger.LogInformation("Orden {OrderId} actualizada con codigo de pago: PaymentStatus={PaymentStatus} , Referencia 1= Aceptada 2=Rechazada", orderBD.OrderId, orderBD.PaymentStatus);

        return order;
    }
}