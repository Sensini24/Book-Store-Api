using BookStoreApi.Models;

namespace BookStoreApi.DTO.UserDTO
{
    public class AddUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Rol { get; set; }
    }
}
