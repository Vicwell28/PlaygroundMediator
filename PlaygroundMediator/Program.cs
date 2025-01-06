using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.Features.Tokens.Handlers;
using PlaygroundMediator.Features.Tokens.Handlers.Commands;
using PlaygroundMediator.PipelineBehavior;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar lectura de JwtSettings desde appsettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 2. Registrar TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

// 3. Agregar Autenticación JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettingsSection!.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false; // En producción, se recomienda true
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettingsSection.Issuer,
        ValidAudience = jwtSettingsSection.Audience,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    // Manejo de eventos para personalizar las respuestas
    o.Events = new JwtBearerEvents
    {
        // Se lanza cuando no hay token o es inválido => 401
        OnChallenge = context =>
        {
            context.HandleResponse();

            var response = new ResponseDto<string>();
            response.SetError(
                message: "No estás autorizado o tu token es inválido/expirado.",
                errors: new List<string> { "Token ausente o inválido" },
                statusCode: 401,
                code: "Unauthorized"
            );

            // Armar JSON y responder con 401
            var json = System.Text.Json.JsonSerializer.Serialize(response);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(json);
        }
    };
});


// 4. Agregar Autorización
builder.Services.AddAuthorization();

// 5. Agregar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configurar Swagger con soporte para Bearer token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. \r\n\r\n" +
                      "Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

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
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Asegurarnos de usar la Autenticación y luego la Autorización
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();