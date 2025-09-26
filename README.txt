Orquestador de Pedidos (OrderOrchestrator) (Versión de Aprendizaje)

Enlace de esquema del Projecto 
Idea Inicial del Flujo de Trabajo 
https://www.mermaidchart.com/d/37e81dc2-37f7-49d7-86b6-93d23b72ebc0

Flujo Final 
https://www.mermaidchart.com/d/b91e1030-a526-459a-b18d-d13cc31efd9a

Este proyecto implementa un flujo de trabajo transaccional de pedidos mediante Azure Durable Functions con .NET 8 Aislado. Su propósito es educativo/empírico; por lo tanto, la configuración de fallas y reintentos es simple.

Seguridad y Punto de Entrada
El punto de acceso inicial a la lógica de negocio es la función HTTP Trigger (OrderReceiver), la cual está protegida por la autenticación de Google.

1. Validación de Autenticación (Google Auth)
Mecanismo: La función OrderReceiver recibe un Google ID Token en la cabecera de la solicitud (Authorization: Bearer <Token>).

Función: La primera tarea del HTTP Trigger es validar este token.

Si el token es válido, se procede a iniciar la orquestación.

Si el token es inválido, la solicitud es rechazada inmediatamente con un código 401 Unauthorized antes de que se inicie cualquier proceso de orquestación.

Alternativa (Easy Auth): El diagrama considera la posibilidad de que Azure App Service Authentication/Authorization (Easy Auth) esté habilitado, delegando la validación del token a la plataforma de Azure.

2. Inicio de la Orquestación
Una vez autenticado, el OrderReceiver inicia el orquestador (OrderOrchestrator) de forma asíncrona, devolviendo al cliente un 202 Accepted y el URI para consultar el estado del pedido (polling).

Configuración del Proyecto

Plataforma y Dependencias
Runtime de Azure Functions: .NET 8 (Aislado)

Lenguaje: C#

Tipo de Function: Durable Function (Orchestrator, Activity)

Persistencia: Entity Framework Core (Azure SQL Database)

Conexión a Base de Datos (Azure SQL)
Servidor: SQL Server en Azure.

Cadena de Conexión: Se usa el nombre SqlConnectionString para la configuración en Program.cs.

JSON

// En local.settings.json (para desarrollo)
{
  "ConnectionStrings": {
    "SqlConnectionString": "Server=tcp:<tu-servidor-azure>.database.windows.net;Initial Catalog=<tu-db-name>;..."
  }
}


Persistencia de Datos: Entity Framework Core (.NET 8 Aislado)
Dada la naturaleza del Worker Aislado en .NET 8, se utiliza el patrón IDbContextFactory<T> para la gestión del contexto de la base de datos.

1. DbContext y Factory
Se asume la existencia de los siguientes componentes para la gestión de datos:

Component
OrderDbContext	
El contexto de base de datos que define los DbSet de las entidades 
(Order, User, ItemOrder).
Inyección de la conexión a Azure SQL.

IDbContextFactory<OrderDbContext>	
Esencial en el modelo Aislado. Permite crear instancias de OrderDbContext de forma segura en las Activity Functions.	
Se registra en Program.cs.

2. Configuración de Servicios (Program.cs)
La configuración de EF Core en el Program.cs del worker aislado debe verse de la siguiente manera:

C#

// Program.cs
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString")));
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();


var app = builder.Build();

app.Run();

3. Implementación de CRUD en Actividades
Las Activity Functions (ej. CreateUser, CreateOrder) consumen la fábrica para obtener un contexto de corta duración:

// Ejemplo de Activity Function
public class CreateUser
{
    private readonly IDbContextFactory<OrderDbContext> _contextFactory;

    public CreateUser(IDbContextFactory<OrderDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [Function("CreateUser")]
    public async Task<UserWithRequestDTO> Run([ActivityTrigger] RequestDTO request)
    {
        // Crear una instancia de DbContext por solicitud (seguro y recomendado)
        using (var context = _contextFactory.CreateDbContext())
        {
            // Lógica de creación de usuario y .SaveChangesAsync()
            // ...
            return new UserWithRequestDTO { /* ... */ };
        }
    }
}

4. Configuración de CORS
Las Azure Functions de .NET Aislado no configuran CORS directamente en el DbContext. El CORS se configura a nivel del Host de Azure Function (en el portal, en el archivo host.json o usando Microsoft.AspNetCore.Cors si se levanta un servidor Kestrel).

Para funciones HTTP (como el OrderReceiver que inicia la orquestación), la configuración correcta de CORS se aplica en el Portal de Azure o en el archivo host.json:

JSON

// host.json (EJEMPLO - la configuración en el portal es más común)
{
  "extensions": {
    "http": {
      "customHeaders": {
        "Access-Control-Allow-Origin": "*" // No recomendado para Producción
      }
    }
  }
}

Flujo de Ejecución y Manejo de Estados

1. Valores de Estado Definidos
Los siguientes valores enteros son utilizados por las actividades UpdateOrderStatatus y ResponseOrder en la propiedad Status de la entidad Order:

0: En Proceso / Pendiente.

1: Aceptado (Pago exitoso, Orden confirmada).

2: Negado (Fallo de inventario, Fallo de pago).

2. Políticas de Reintento
Nota: Al ser un proyecto meramente empírico y de aprendizaje, no se han configurado políticas de reintento (RetryOptions) en ninguna actividad.

Implicación: Si una Activity Function (ej. CreateUser) falla debido a un error transitorio de red o de base de datos, el orquestador fallará inmediatamente en lugar de intentar la operación de nuevo. Esto no es adecuado para producción.

3. Servicios Externos
Servicio de Pago (CheckPayment): Se asume que esta actividad simula la validación de un pago, ya que no se integra un servicio de pago real. Por lo tanto, no se requiere configuración de integración de terceros.