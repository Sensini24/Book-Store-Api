using BookStoreApi.Models;

namespace BookStoreApi.DTO.UserDTO
{
    public class GetUserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
