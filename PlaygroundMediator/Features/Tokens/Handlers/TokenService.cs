using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlaygroundMediator.DTOs;
using PlaygroundMediator.Features.Tokens.Handlers.Commands;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlaygroundMediator.Features.Tokens.Handlers
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(
            string userName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions)
        {
            // 1. Crear lista de claims base
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userName)
            };

            // 2. Agregar cada rol al token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Agregar permisos como claims personalizados
            //    Puedes usar la key "permission" o la que decidas.
            //    También se podría usar un ClaimType específico.
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            // 4. Generar la llave simétrica (Key)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Crear la descripción del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            // 6. Generar el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 7. Retornar el token en formato string
            return tokenHandler.WriteToken(token);
        }
    }
}