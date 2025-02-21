using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BookStoreApi.Models;
namespace BookStoreApi.Sevices
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _db;

        public TokenService(IConfiguration configuration, DataContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        public string CrearToken(int idUser)
        {
            var key = _configuration["Jwt:Key"]; /*GetValue<string>("Jwt:Key")*/

            if (key == null)
            {
                return "No se pudo crear el token por falta de información relevante.";
            }


            //Convierte la key de appsettings en un conjunto de bytes para que pueda ser codificado
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var userRol = _db.Users.Where(u => u.Id == idUser).FirstOrDefault();
            if (userRol == null) 
            {
                return "El usuario no existe.";
            }
            //Creacion de claims, en este caso estoy usando el id del usuario.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, idUser.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role,  userRol.Rol)
            };

            //Una vez convertida la key en bytes y codificada, puede usarse para verificar y firmar el token. Y eso hace SymmetricSecurity, verifica y firma el codigo criptografiado a bytes previamente.
            var symetricKey = new SymmetricSecurityKey(keyBytes);

            //Firmar el token con el algoritmo escogido y symetrickey
            var credentials = new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
