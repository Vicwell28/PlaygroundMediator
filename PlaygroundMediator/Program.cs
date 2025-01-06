using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.Extensions;
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
        // OnAuthenticationFailed
        OnAuthenticationFailed = async context =>
        {
            // Ocurre cuando se encontró un token en la petición,
            // pero falló la validación: firma inválida, token expirado, etc.

            // Aquí NO tienes JwtBearerChallengeContext, sino AuthenticationFailedContext.
            // Sí puedes "manejar" la respuesta manualmente.
            //context.HandleResponse();

            var response = new ResponseDto<string>();
            response.SetError(
                message: "Fallo la autenticación (token inválido o expirado).",
                errors: new List<string> { context.Exception.Message },
                statusCode: 401,
                code: "AuthFailed"
            );

            var json = System.Text.Json.JsonSerializer.Serialize(response);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        },

        // OnChallenge
        OnChallenge = async context =>
        {
            // Se dispara cuando NO hay token o es claramente inválido
            // y el framework está a punto de retornar 401 Unauthorized.
            // Aquí, a diferencia de OnAuthenticationFailed, el token puede ni existir,
            // o podría estar mal formado, etc.
            context.HandleResponse();

            var response = new ResponseDto<string>();
            response.SetError(
                message: "No estás autorizado o no se encontró el token.",
                errors: new List<string> { "Token ausente o inválido" },
                statusCode: 401,
                code: "Unauthorized"
            );

            var json = System.Text.Json.JsonSerializer.Serialize(response);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        },

        // OnForbidden
        OnForbidden = async context =>
        {
            // Se dispara cuando el usuario sí está autenticado,
            // pero NO tiene los permisos o roles necesarios => 403 Forbidden.
            //
            // Este contexto es ForbiddenContext y NO incluye HandleResponse().
            // Aun así, puedes escribir la respuesta personalizada.
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = new ResponseDto<string>();
            response.SetError(
                message: "Acceso denegado, no tienes permisos para este recurso.",
                errors: new List<string> { "Forbidden" },
                statusCode: 403,
                code: "Forbidden"
            );

            var json = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    };
});



// 4. Agregar Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddCustomAuthorizationPolicies();
});

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