using BookStoreApi.Models;

namespace BookStoreApi.DTO.CartItemsDTO
{
    public class AddCartItemDTO
    {
        public int CartId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}
