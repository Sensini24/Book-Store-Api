namespace BookStoreApi.DTO.OrderDTO;

public class AddOrderDetailDTO
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}