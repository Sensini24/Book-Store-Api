using BookStoreApi.Models;

namespace BookStoreApi.DTO.BookDTO
{
    public class AddBookDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public IFormFile? Image { get; set; }
        public int IdGenre { get; set; }
        public List<string> Tags { get; set; }
    }
}
