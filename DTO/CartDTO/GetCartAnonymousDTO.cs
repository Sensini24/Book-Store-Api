using BookStoreApi.Models;

namespace BookStoreApi.DTO.CartDTO
{
    public class GetCartAnonymousDTO
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<CartItem> CartItems { get; set; }
        
        public virtual ICollection<Book> Books { get; set; } 
    }
}
