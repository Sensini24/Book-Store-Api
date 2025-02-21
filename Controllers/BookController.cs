using System.Security.Claims;
using BookStoreApi.DTO.BookDTO;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly DataContext _db;
        private readonly ILogger<BookController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserController _userController;
        
        public BookController(DataContext db, ILogger<BookController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // [Authorize(Roles = "Cliente")]
        [HttpGet]
        [Route("getBooks")]
        public async Task<IActionResult> Get()
        {
            // Obtener el userId y sessionId
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = Request.Cookies["SessionId"];

            
            bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);
            bool hasSession = !string.IsNullOrEmpty(sessionId);

            if (!isUserLoggedIn && !hasSession)
            {
                return Ok(new { success = true, message = "Libros Obtenidos", productos = _db.Books.ToList() });
            }

            int? idUser = isUserLoggedIn ? int.Parse(currentUserName) : null;

            
            var productos = await (
                from x in _db.Books
                select new
                {
                    IdBook = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Author = x.Author,
                    Price = x.Price,
                    Stock = x.Stock,
                    ImagePath = x.ImagePath,
                    Created = x.Created,
                    IdGenre = x.IdGenre,
                    Tags = x.Tags,
                    IsInCart = _db.CartItems.Any(ci =>
                        ci.BookId == x.Id &&
                        (isUserLoggedIn ? ci.Cart.UserId == idUser : ci.Cart.SessionId == sessionId)) // Verifica si está en el carrito
                }
            ).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Libros Obtenidos",
                productos
            });
        }


        [Authorize]
        [HttpPost]
        [Route("addBook")]
        public async Task<IActionResult> Add([FromForm] AddBookDTO bookdto)
        {
            _logger.LogDebug($"cuerpo de libro: {bookdto.IdGenre}");
            _logger.LogDebug($"Image de libro: {bookdto.Image}");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogDebug($"Modelo inválido");
                    return BadRequest(ModelState);
                }

                await using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var fileName = bookdto.Image.FileName;
                        string rutaRelativa = $"/covers/{fileName}";
                        _logger.LogInformation($"Ruta relativa para guardar: {rutaRelativa}");
                        var book = new Book()
                        {
                            
                            Title = bookdto.Title,
                            Description = bookdto.Description,
                            Author = bookdto.Author,
                            Price = bookdto.Price,
                            Stock = bookdto.Stock,
                            ImagePath = rutaRelativa,
                            IdGenre = bookdto.IdGenre,
                            Tags = bookdto.Tags,
                        };

                        await SaveImage(bookdto.Image);
                        
                        await _db.Books.AddAsync(book);
                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return Ok(new { success = true, message = "Libro guardado exitosamente", bookdto });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"Rollbacj hecho");
                        await transaction.RollbackAsync();
                        return BadRequest(new{ex.Message});
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error de servidor: {ex.Message}");
                return BadRequest(new{ex.Message});
            }
        }

        [Authorize]
        [HttpPut]
        [Route("editBook")]
        public async Task<IActionResult> Put([FromBody] EditBookDTO bookdto)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var book = new Book()
            {
                Id = bookdto.Id,
                Title = bookdto.Title,
                Description = bookdto.Description,
                Author = bookdto.Author,
                Price = bookdto.Price,
                Stock = bookdto.Stock,
                ImagePath = bookdto.ImagePath,
                IdGenre = bookdto.IdGenre,
                Tags = bookdto.Tags,
            };

            _db.Books.Update(book);
            await _db.SaveChangesAsync();
            

            return Ok(bookdto);
        }


        public async Task<IActionResult> SaveImage(IFormFile file)
        {
            // var rutaCovers = "D:\\ProjectsDocs\\Covers";
            var webpathRuta = _webHostEnvironment.WebRootPath;
            var rutaCovers = Path.Combine(webpathRuta, "covers");
            var image = file;
            var imageName = image.FileName;

            if (System.IO.File.Exists(Path.Combine(rutaCovers, imageName)))
            {
                _logger.LogDebug($"Mensaje de image file: La imagen ya existe");
                return BadRequest(new {success=false, message="Imagen ya existe", fileName=imageName });
            }
            
            var rutaArchivo = Path.Combine(rutaCovers, imageName);
            await using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            
            return Ok(new{success = true, message = "La imagen fue subida correctamente", fileName=imageName });
            
        }
    }
}
