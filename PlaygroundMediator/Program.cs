using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PlaygroundMediator.PipelineBehavior;
using System.Reflection;
using FluentValidation.Internal;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
builder.Services.AddControllers();

// Registrar MediatR
//Básicamente, le dice a MediatR que busque en un ensamblado (en este caso, el que contiene la clase Program)
//todas las clases que implementen IRequestHandler<TRequest,TResponse>, INotificationHandler<TNotification>
//y demás tipos de handlers, para inyectarlas automáticamente.

// 1. Registrar MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 2. Registrar FluentValidation (si lo usas con extension method)
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);


// 3. Registrar el Pipeline Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>),typeof(ValidationBehavior<,>));


// Configurar Swagger si es necesario
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();