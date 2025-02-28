using BookStoreApi.Models;

namespace BookStoreApi.DTO.OrderDTO;

public class AddOrderDTO
{
    public decimal Total { get; set; }
    public ICollection<AddOrderDetailDTO> OrderDetails { get; set; }
}