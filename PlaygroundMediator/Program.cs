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

// 3. Agregar Autenticaci�n JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettingsSection!.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false; // En producci�n, se recomienda true
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
            // Ocurre cuando se encontr� un token en la petici�n,
            // pero fall� la validaci�n: firma inv�lida, token expirado, etc.

            // Aqu� NO tienes JwtBearerChallengeContext, sino AuthenticationFailedContext.
            // S� puedes "manejar" la respuesta manualmente.
            //context.HandleResponse();

            var response = new ResponseDto<string>();
            response.SetError(
                message: "Fallo la autenticaci�n (token inv�lido o expirado).",
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
            // Se dispara cuando NO hay token o es claramente inv�lido
            // y el framework est� a punto de retornar 401 Unauthorized.
            // Aqu�, a diferencia de OnAuthenticationFailed, el token puede ni existir,
            // o podr�a estar mal formado, etc.
            context.HandleResponse();

            var response = new ResponseDto<string>();
            response.SetError(
                message: "No est�s autorizado o no se encontr� el token.",
                errors: new List<string> { "Token ausente o inv�lido" },
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
            // Se dispara cuando el usuario s� est� autenticado,
            // pero NO tiene los permisos o roles necesarios => 403 Forbidden.
            //
            // Este contexto es ForbiddenContext y NO incluye HandleResponse().
            // Aun as�, puedes escribir la respuesta personalizada.
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



// 4. Agregar Autorizaci�n
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
//B�sicamente, le dice a MediatR que busque en un ensamblado (en este caso, el que contiene la clase Program)
//todas las clases que implementen IRequestHandler<TRequest,TResponse>, INotificationHandler<TNotification>
//y dem�s tipos de handlers, para inyectarlas autom�ticamente.

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

// Asegurarnos de usar la Autenticaci�n y luego la Autorizaci�n
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();