using DurableFunctionExample.DBContext;
using DurableFunctionExample.DTO;
using DurableFunctionExample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DurableFunctionExample.Funtions.Activities;

public class CreateUser
{
    private readonly ILogger<ValidateInventory> _logger;
    private readonly OrderDbContext _db;

    public CreateUser(ILogger<ValidateInventory> logger, OrderDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    [Function(nameof(CreateUser))]
    public async Task<UserWithRequestDTO> Run([ActivityTrigger] RequestDTO RequestUser)
    {
        
        try
        {
            _logger.LogInformation("CrearUser {UserEmail}", RequestUser.UserEmail);
            // Verificar si ya existe el usuario por correo
            var usuarioExistente = await _db.Users
                .FirstOrDefaultAsync(u => u.Correo == RequestUser.UserEmail);

            if (usuarioExistente != null)
            {
                _logger.LogInformation("El usuario ya existe");
                var usuarioWithRequest = new UserWithRequestDTO
                {
                    UserId = usuarioExistente.Id,
                    Request = RequestUser
                };
                return usuarioWithRequest;
            }

            //Sino crear el nuevo usuario
            var usuario = new User
            {
                Correo = RequestUser.UserEmail,
                Nombre = RequestUser.UserName
            };

            _db.Users.Add(usuario);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Usuario creado exitosamente {Id} ", usuario.Id);

            return new UserWithRequestDTO { UserId = usuario.Id, Request = RequestUser };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear o buscar usuario ");
            throw;
        }
    }
}
