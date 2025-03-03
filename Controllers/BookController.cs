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
                var books = await (
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
                        Tags = x.Tags
                    }
                ).ToListAsync();
                return Ok(new { success = true, message = "Libros Obtenidos", productos = books });
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
        
        
        [HttpGet]
        [Route("getBooksById/{idBook}")]
        // public async Task<IActionResult> Get([FromBody] int idBook)
        public async Task<IActionResult> Get([FromRoute] int idBook)
        {
            if (idBook == null || idBook <=0)
            {
                return BadRequest(new
                {
                    success = true,
                    message = "IdBook no existe"
                });
            }
            // Obtener el userId y sessionId
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = Request.Cookies["SessionId"];

            
            bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);
            bool hasSession = !string.IsNullOrEmpty(sessionId);

            if (!isUserLoggedIn && !hasSession)
            {
                
                var books = await (
                    from x in _db.Books
                    where x.Id == idBook
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
                        Tags = x.Tags
                    }
                ).ToListAsync();
                return Ok(new { success = true, message = "Libros Obtenidos", productos = books });
            }

            int? idUser = isUserLoggedIn ? int.Parse(currentUserName) : null;

            
            var productos = await (
                from x in _db.Books
                where x.Id == idBook
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

        [Authorize(Roles = "Administrador")]
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

        [Authorize(Roles = "Administrador")]
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
            var rutaArchivo = Path.Combine(rutaCovers, imageName);
            
            

            if (System.IO.File.Exists(Path.Combine(rutaCovers, imageName)))
            {
                _logger.LogDebug($"Mensaje de image file: La imagen ya existe");
                return BadRequest(new {success=false, message="Imagen ya existe", fileName=imageName });
            }

            await using (Stream inputStream = image.OpenReadStream())
            // await using (FileStream ouputStreamImage = new FileStream(rutaArchivo, FileMode.Create))
            
            /*
             *EXPLICACION:
             * 1. Se convierte el File en un OpenReadStream para poder manipularlo.
             * 2. Se crea un FileStream vacío en la ruta donde se gurdará el archivo o imagen.
             * 3. En este caso se opta por chunks o pedazos para reducir carga en la memoria
             * 4. Se crea un buffer y se asigna una cantidad limitada que se llene en pedazos
             * 5. Los bytesLeidos son para ver cuanto debe almacenarse en buffer
             * 6. Se lee el stream y se pasa a buffer. La iteracion acaba cuando no hay datos que leer.
             * 7. Se escribe en el stream creado en ruta, y los bytesLeidos indican cuanto se debe escibir a ese stream.
             */
            await using(FileStream ouputStream =  System.IO.File.Create(rutaArchivo))
            { 
                byte[] buffer = new byte[4096];
                int bytesLeidos;
                while ((bytesLeidos = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ouputStream.WriteAsync(buffer, 0, bytesLeidos);
                }
            }
            // await using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            // {
            //     await image.CopyToAsync(stream);
            // }
            
            return Ok(new{success = true, message = "La imagen fue subida correctamente", fileName=imageName });
            
        }
    }
}
