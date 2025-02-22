using BookStoreApi.DTO.AuthDTO;
using BookStoreApi.Models;
using BookStoreApi.Sevices;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly DataContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(DataContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDTO logindto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isUser = _db.Users.Any(e=>e.Email == logindto.Email && e.Password == logindto.Password);
            if (isUser)
            {
                var user = _db.Users.Where(u=>u.Email== logindto.Email).FirstOrDefault();
                var token = _tokenService.CrearToken(user.Id);

                addTokenToCookie(token);

                return Ok(new { success = true, message = "Usuario aceptado", Token = token });
            }
            return NotFound(new
            {
                success = false,
                message= "Crendenciales incorrectas",
                data = logindto
            });
        }

        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            foreach (var cookie in Request.Cookies)
            {
                if (cookie.Key == "tokenUser")
                {
                    Response.Cookies.Delete(cookie.Key, new CookieOptions
                    {
                        Path = "/",
                        Secure = true,
                        SameSite = SameSiteMode.None
                    });
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Cookie eliminado",
                        value = cookie.Value,
                        Key = cookie.Key
                    });
                    
                }
                
                
            }
            
            return NotFound(new
            {
                success = false,
                message = "Ningún usuario logeado actualmente",
            });

            // return BadRequest();
            
        }

        string addTokenToCookie(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return "El token esta vacío";
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("tokenUser", token, cookieOptions);
            return "Token almacenado en cookies";
        }
    }
}
