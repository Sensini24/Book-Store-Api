namespace BookStoreApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }

        //Segunda migracion: cartId puede ser null
        public int? CartId { get; set; }
        public Cart Cart { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
