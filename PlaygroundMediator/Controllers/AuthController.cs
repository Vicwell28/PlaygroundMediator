using Microsoft.AspNetCore.Mvc;
using PlaygroundMediator.Features.Tokens.Handlers.Commands;

namespace PlaygroundMediator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. Normalmente validaríamos credenciales con Identity o en la DB
            //    Aquí simulamos que el login es correcto solo si userName=demo y password=demo
            if (request.UserName == "demo" && request.Password == "demo")
            {
                // 2. Crear lista de roles y permisos ficticios
                var roles = new List<string> { "Admin", "User" };
                var permissions = new List<string> { "read_products", "create_products" };

                // 3. Generar token con roles y permisos
                var token = _tokenService.GenerateToken(
                    request.UserName,
                    roles,
                    permissions
                );

                // 4. Retornar el token en la respuesta
                return Ok(new { Token = token });
            }

            // Credenciales inválidas
            return Unauthorized();
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}