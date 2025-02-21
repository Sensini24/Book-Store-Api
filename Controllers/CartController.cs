using System.Security.Claims;
using BookStoreApi.DTO.BookDTO;
using BookStoreApi.DTO.CartDTO;
using BookStoreApi.DTO.CartItemsDTO;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly DataContext _db;
        private readonly UserController uc;
        public CartController(DataContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("getCart/{sessionid}")]
        public async Task<IActionResult> Get(string sessionid)
        {
            if(sessionid == null)
            {
                return NotFound();
            }

            var cart = await _db.Carts.Include(x=>x.CartItems).Where(i=>i.SessionId == sessionid).FirstOrDefaultAsync();
            if (cart == null)
            {
                return NotFound("No se encontró ese card con se session id");
            }

            var cartdto = new GetCartAnonymousDTO()
            {
                Id = cart.Id,
                SessionId = cart.SessionId,
                CreatedAt = cart.CreatedAt,
                CartItems = cart.CartItems,
            };

            return Ok(cartdto);
        }

        [HttpGet]
        [Route("getCartByUser/{userid}")]
        public async Task<IActionResult> Get(int userid)
        {
            if (userid == 0)
            {
                return NotFound();
            }

            var cart = await _db.Carts.Include(x => x.CartItems).Where(i => i.UserId == userid).FirstOrDefaultAsync();
            if (cart == null)
            {
                return NotFound("No se encontró ese card con se session id");
            }

            var cartdto = new GetCartAnonymousDTO()
            {
                Id = cart.Id,
                SessionId = cart.SessionId,
                CreatedAt = cart.CreatedAt,
                CartItems = cart.CartItems,
            };

            return Ok(cartdto);
        }

        [HttpGet]
        [Route("getUserCart")]
        public async Task<IActionResult> Get()
        {
            var sessionId = Request.Cookies["SessionId"];
            var currentUserName =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = new Cart();
            
            if (String.IsNullOrEmpty(sessionId))
            {
                NotFound(new
                {
                    success=false,
                    message = "No se encontró ese card con se session id"
                });
            }
            else
            {
                 cart = await _db.Carts.Include(x => x.CartItems)
                     .Where(i => i.SessionId == sessionId).FirstOrDefaultAsync();
                 if (cart == null)
                 {
                     NotFound(new
                     {
                         sucess = false,
                         message = "No se encontró ese card con se session id"
                     });
                 }
            }
            
            if (String.IsNullOrEmpty(currentUserName))
            {
                return NotFound(new
                {
                    success = false,
                    message = "Ningún usuario registrado"
                });
            }
            else
            {
                int idUser = int.Parse(currentUserName);
            
                if (idUser == 0)
                {
                    NotFound();
                }
                cart = await _db.Carts.Include(x => x.CartItems).Where(i => i.UserId == idUser).FirstOrDefaultAsync();
                if (cart == null)
                {
                    return NotFound(new
                    {
                        sucess = false,
                        message = "Este usuario no tiene ningun carrito de compra"
                    });
                }
            }

            var cartdto = new GetCartAnonymousDTO()
            {
                Id = cart.Id,
                SessionId = cart.SessionId,
                CreatedAt = cart.CreatedAt,
                CartItems = cart.CartItems,
                Books = _db.Books.Where(x=>cart.CartItems.Select(x=>x.BookId).Distinct().Contains(x.Id)).ToList(),
            };
            
            // IEnumerable<char> ids = cartdto.CartItems.SelectMany(x=>x.BookId).Distinct();

            return Ok(new
            {
                success = true,
                message = "Se encontraron items",
                cartdto
            });

        }
        [HttpPost]
        [Route("addUserCart/{BookId:int}")]
        public async Task<IActionResult> Add(int BookId)
        {
            if (BookId < 1)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El Id no es válido"
                });
            }
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = Request.Cookies["SessionId"];

            bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);
            bool hasSession = !string.IsNullOrEmpty(sessionId);
            
            var cart = new Cart();
            var cartItems = new List<CartItem>();
            int? idUser = int.Parse(currentUserName);
            
            if (isUserLoggedIn || hasSession)
            {
                
                if (isUserLoggedIn)
                {
                    cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x=>x.UserId == idUser) ?? new Cart
                    {
                        UserId = idUser,
                        SessionId = "",
                        CreatedAt = DateTime.Now,
                        CartItems = new List<CartItem>()
                    };


                    // Ahora en el caso de que el item ya exista.
                    var item =  cart.CartItems.FirstOrDefault(x => x.BookId == BookId);
                    var book = _db.Books.FirstOrDefault(x => x.Id == BookId);
                    if (item == null)
                    {
                        var cartItem = new CartItem()
                        {
                            CartId = cart.Id,
                            BookId = book.Id,
                            Quantity = 1,
                            UnitPrice = _db.Books.Where(x=>x.Id == book.Id).FirstOrDefault().Price,
                        };
                        cart.CartItems.Add(cartItem);
                        await _db.SaveChangesAsync();
                        return Ok(new
                        {
                            sucess = true,
                            message = "Item guardado en carrito",
                        });
                    }

                    return Ok(new
                    {
                        sucess = true,
                        message = "Se encontraron items",
                    });

                }

                if (hasSession)
                {
                    cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x=>x.SessionId == sessionId)?? new Cart
                    {
                        UserId = null,
                        SessionId = sessionId,
                        CreatedAt = DateTime.Now,
                        CartItems = new List<CartItem>()
                    };
                    
                    // Ahora en el caso de que el item ya exista.
                    var item =  cart.CartItems.FirstOrDefault(x => x.BookId == BookId);
                    if (item != null)
                    {
                        var cartItem = new CartItem()
                        {
                            CartId = item.CartId,
                            BookId = item.BookId,
                            Quantity = 1,
                            UnitPrice = _db.Books.Where(x=>x.Id == item.BookId).FirstOrDefault().Price,
                        };
                        cart.CartItems.Add(cartItem);
                        
                        return Ok(new
                        {
                            sucess = true,
                            message = "Item guardado en carrito",
                        });
                    }

                    return Ok(new
                    {
                        sucess = true,
                        message = "Se encontraron items",
                    });
                }
                // else
                // {
                //     sessionId = Guid.NewGuid().ToString();
                //     var cookieOptions = new CookieOptions
                //     {
                //         HttpOnly = true,
                //         IsEssential = true,
                //         Secure = true,
                //         SameSite = SameSiteMode.None,
                //         Expires = DateTime.UtcNow.AddDays(7)
                //     };
                //     Response.Cookies.Append("SessionId", sessionId, cookieOptions);
                // }
                
                
                
            }

            
            
            // var cart = new Cart();
            // var cartItems = new List<CartItem>();
            // if (idUser>0)
            // {
            //     //return Ok(new {message="Si hay valor",  value=cartADTO.UserId.Value });
            //     cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.UserId == idUser);
            //     cartItems = _db.CartItems.Where(x => x.CartId == cart.Id).ToList();
            // }
            // else
            // {
            //     
            //     // En caso de que no se tenga un session id
            //     if (!hasSession)
            //     {
            //         sessionId = Guid.NewGuid().ToString();
            //         var cookieOptions = new CookieOptions
            //         {
            //             HttpOnly = true,
            //             IsEssential = true,
            //             Secure = true,
            //             SameSite = SameSiteMode.None,
            //             Expires = DateTime.UtcNow.AddDays(7)
            //         };
            //         Response.Cookies.Append("SessionId", sessionId, cookieOptions);
            //     }
            //
            //     // // Buscar si existe un carrito con el SessionId
            //     // cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.SessionId == sessionId);
            //     cartItems = _db.CartItems.Where(x => x.CartId == cart.Id).ToList();
            // }
            

            // // Si no existe, crear el carrito
            // if (cart == null)
            // {
            //     cart = new Cart
            //     {
            //         UserId = idUser > 0 ? idUser : (int?)null,
            //         SessionId = sessionId == null ? "": sessionId,
            //         CreatedAt = DateTime.Now,
            //         CartItems = new List<CartItem>()
            //     };
            //
            //     _db.Carts.Add(cart);
            //     await _db.SaveChangesAsync();
            //     //En caso de que se este agregando items con un user id.
            //     if (cart.UserId > 0)
            //     {
            //         var user = await _db.Users.Where(i => i.Id == idUser).FirstOrDefaultAsync();
            //         if (user != null)
            //         {
            //             user.CartId = cart.Id;
            //         }
            //         
            //     }
            // }
            //
            // // Llenar de ítems seleccionados el carrito
            // foreach (var item in cartItems)
            // {
            //     // Buscar si el ítem ya existe en el carrito actual
            //     var existItem = cart.CartItems.FirstOrDefault(x => x.BookId == item.BookId);
            //
            //     if (existItem != null)
            //     {
            //         // Si el ítem ya existe, actualizar la cantidad
            //         existItem.Quantity += item.Quantity;
            //     }
            //     else
            //     {
            //         // Si no existe, agregarlo
            //         var newCartItem = new CartItem
            //         {
            //             CartId = cart.Id,
            //             BookId = item.BookId,
            //             Quantity = item.Quantity,
            //             UnitPrice = item.UnitPrice
            //         };
            //         cart.CartItems.Add(newCartItem);
            //     }
            // }

            // Guardar los cambios en la base de datos
            await _db.SaveChangesAsync();

            // Retornar el carrito actualizado junto con el SessionId
            return Ok(new
            {
                SessionId = cart.SessionId,
                Cart = new
                {
                    cart.Id,
                    cart.UserId,
                    cart.SessionId,
                    cart.CreatedAt,
                    CartItems = cart.CartItems.Select(ci => new
                    {
                        ci.Id,
                        ci.BookId,
                        ci.Quantity,
                        ci.UnitPrice
                    })
                }
            });
        }

        [HttpPost]
        [Route("addCart")]
        public async Task<IActionResult> Add([FromBody] AddCartAnonymousDTO cartADTO)
        {
            var cart = new Cart();
            if (cartADTO.UserId.HasValue)
            {
                //return Ok(new {message="Si hay valor",  value=cartADTO.UserId.Value });
                cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.UserId == cartADTO.UserId);
            }
            else
            {
                // En caso de que no se tenga un session id
                if (String.IsNullOrEmpty(cartADTO.SessionId))
                {
                    cartADTO.SessionId = Guid.NewGuid().ToString();
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };
                    Response.Cookies.Append("SessionId", cartADTO.SessionId, cookieOptions);
                }

                // Buscar si existe un carrito con el SessionId
                cart = await _db.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.SessionId == cartADTO.SessionId);
            }
            

            // Si no existe, crear el carrito
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = cartADTO.UserId > 0 ? cartADTO.UserId : (int?)null,
                    SessionId = cartADTO.SessionId == null ? "": cartADTO.SessionId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
                //En caso de que se este agregando items con un user id.
                if (cart.UserId > 0)
                {
                    var user =_db.Users.Where(i => i.Id == cart.UserId).FirstOrDefault();
                    if (user != null)
                    {
                        user.CartId = cart.Id;
                    }
                    
                }
            }

            // Llenar de ítems seleccionados el carrito
            foreach (var item in cartADTO.CartItems)
            {
                // Buscar si el ítem ya existe en el carrito actual
                var existItem = cart.CartItems.FirstOrDefault(x => x.BookId == item.BookId);

                if (existItem != null)
                {
                    // Si el ítem ya existe, actualizar la cantidad
                    existItem.Quantity += item.Quantity;
                }
                else
                {
                    // Si no existe, agregarlo
                    var newCartItem = new CartItem
                    {
                        CartId = cart.Id,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    cart.CartItems.Add(newCartItem);
                }
            }

            // Guardar los cambios en la base de datos
            await _db.SaveChangesAsync();

            // Retornar el carrito actualizado junto con el SessionId
            return Ok(new
            {
                SessionId = cart.SessionId,
                Cart = new
                {
                    cart.Id,
                    cart.UserId,
                    cart.SessionId,
                    cart.CreatedAt,
                    CartItems = cart.CartItems.Select(ci => new
                    {
                        ci.Id,
                        ci.BookId,
                        ci.Quantity,
                        ci.UnitPrice
                    })
                }
            });
        }


        [HttpGet]
        [Route("getCookie")]
        public IActionResult GetCookie()
        {
            //var cookies = Request.Cookies["SessionId"];
            var cookies = Request.Cookies;
            if (cookies != null)
            {
                var cookieSession = Request.Cookies["SessionId"];
                return Ok( new
                {
                    message= "Cookie encontrada",
                    cookieSession
                });
            }
            return NotFound("No existen cookies");
        }


        [HttpPut]
        [Route("editItems")]
        public async Task<IActionResult> EditQuantity([FromBody] EditQuantityDTO editQuantityDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Modelo Inválidos"
                    });
                }
                
                int bookId = editQuantityDTO.BookId; 
                int quantity = editQuantityDTO.Quantity;
                    
                if (bookId <= 0)
                {
                    return NotFound(new
                    {
                        message = "Ingrese un valor",
                        success = false,
                    });
                }

                var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (String.IsNullOrEmpty(currentUserName))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Ningún usuario registrado"
                    });
                }


                int idUser = int.Parse(currentUserName);
                var cartItem = await _db.Carts.Where(u => u.UserId == idUser).Select(i => i.CartItems)
                    .FirstOrDefaultAsync();
                if (cartItem == null)
                {
                    return NotFound(new
                    {
                        sucess = false,
                        message = "Este usuario no tiene ningun carrito de compra"
                    });
                }

                var item = cartItem.FirstOrDefault(x => x.BookId == bookId);

                item.Quantity = quantity;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Se editó la cantidad del item.",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
