using BookStoreApi.Models;

namespace BookStoreApi.Sevices
{
    public class Utils
    {
        private readonly DataContext _context;

        public Utils(DataContext context)
        {
            _context = context;
        }

        public string GetUserRol(int id)
        {
            if (id == 0)
            {
                return "No existe este id";
            }
            var rol = _context.Users.Where(u => u.Id == id).Select(r => r.Rol).ToString();

            if (rol == null)
            {
                return "No se encontro el usuario.";
            }

            return rol;
        }
    }
}
