namespace BookStoreApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImagePath { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;   
        public int IdGenre { get; set; } 
        public virtual Genre Genre {get; set;}
        public List<string> Tags { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Comment>? Comments { get; set; }

    }
}
