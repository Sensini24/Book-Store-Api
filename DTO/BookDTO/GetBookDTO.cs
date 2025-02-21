using BookStoreApi.Models;

namespace BookStoreApi.DTO.BookDTO
{
    public class GetBookDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImagePath { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int IdGenre { get; set; }
        public List<string> Tags { get; set; }
    }
}
