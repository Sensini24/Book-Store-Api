using BookStoreApi.DTO.GenreDTO;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : Controller
    {
        private readonly DataContext _db;

        public GenreController(DataContext db)
        {
            _db = db;
        }
        //[HttpGet]
        //[Route("getGenre")]
        //public async Task<IActionResult> Get()
        //{
        //    var genres = _db.Genres.ToList();

        //    return Ok(genres);
        //}

        [HttpGet]
        [Route("getGenre")]
        public async Task<IActionResult> Get()
        {
            var genres = await _db.Genres.ToListAsync();
            var genresDTO = genres.Select(x => new GetGenreDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
            });
            return Ok(new {success = true, generos = genresDTO});
        }

        [HttpPost]
        [Route("addGenre")]
        public async Task<IActionResult> Add([FromBody] AddGenreDTO genre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var genreAdd = new Genre()
            {
                Name = genre.Name,
                Description = genre.Description
            };

            await _db.Genres.AddAsync(genreAdd);
            await _db.SaveChangesAsync();

            return Ok(genre);
        }
    }
}
