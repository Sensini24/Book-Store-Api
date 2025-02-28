using System.Security.Claims;
using BookStoreApi.DTO.AuthDTO;
using BookStoreApi.Models;
using BookStoreApi.Sevices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Login([FromBody] LoginDTO logindto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var isUser = await _db.Users.AnyAsync(e=>e.Email == logindto.Email && e.Password == logindto.Password);
            if (isUser)
            {
                var user = await _db.Users.Where(u=>u.Email== logindto.Email).FirstOrDefaultAsync();
                var token = _tokenService.CrearToken(user.Id);
                IActionResult result = NotFound(new
                {
                    success = false,
                    message = "Este usuario no es cliente."
                });
                SetAuthCookie(token);
                if (User.FindFirst(ClaimTypes.Role)?.Value == "Cliente")
                {
                    result = await AddItemsSession(user.Id);
                }
                
                
                return Ok(new { success = true, message = "Usuario aceptado", Token = token, result });
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

        
        private void SetAuthCookie(string token)
        {
            Response.Cookies.Append("tokenUser", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
        private async Task<IActionResult>  addTokenToCookie(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound(new
                {
                    success = false,
                    message = "No se obtuvo el token",
                });
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
            
            return Ok(new
            {
                success = true,
                message = "Token guardado en cookies"
            });
        }
        
        
        private async Task<IActionResult> AddItemsSession(int usuarioId)
        {
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = Request.Cookies["SessionId"];

            bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);
            bool hasSession = !string.IsNullOrEmpty(sessionId);

            int idUser = usuarioId;
            var idCart = _db.Carts.Where(c => c.UserId == idUser).Select(i=>i.Id).FirstOrDefault();
            ICollection<CartItem>? cartItems; 
            var cart = new Cart();
            
            if (idCart == null)
            {
                cart = new Cart
                {
                    UserId = idUser,
                    SessionId = "",
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();

                cartItems = cart.CartItems.ToList();
            }
            else
            {
                cartItems = _db.CartItems.Where(x => x.CartId == idCart).ToList();
            }
            
            if (hasSession)
            {
                var idCartSession = _db.Carts.Where(c => c.SessionId == sessionId).Select(i=>i.Id).FirstOrDefault();
                var cartItemsSession = _db.CartItems.Where(x => x.CartId == idCartSession);
                var newItems = new List<CartItem>(); 
                
                bool itemExistente = false;
                Dictionary<int,int> nombresExistentes = new Dictionary<int,int>();
                foreach (var itemsSession in cartItemsSession)
                {
                    foreach (var itemUser in cartItems)
                    {
                        if (itemsSession.BookId == itemUser.BookId)
                        {
                            itemUser.Quantity += itemsSession.Quantity;
                            nombresExistentes.Add(itemsSession.BookId, itemsSession.Quantity);
                        }
                        
                    }

                    if (!nombresExistentes.ContainsKey(itemsSession.BookId))
                    {
                        newItems.Add(new CartItem
                        {
                            CartId = cartItems.FirstOrDefault()?.CartId ?? idCart,
                            BookId = itemsSession.BookId,
                            Quantity = itemsSession.Quantity,
                            UnitPrice = _db.Books.Where(x => x.Id == itemsSession.BookId).Select(p => p.Price)
                                .FirstOrDefault()
                        }); 
                    }
                    
                    

                }
                _db.CartItems.AddRange(newItems);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Los items de session fueron desplazados a el carrito de usuarios."
                });
            }
            return NotFound(new
            {
                success = false,
                message = "Session no encontrada."
            });
        }
    }
}
