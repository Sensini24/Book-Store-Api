using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookStoreApi.DTO.UserDTO;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly DataContext _db;

        public UserController(DataContext db)
        {
            _db = db;
        }
        [Authorize]
        [HttpGet]
        [Route("getUsers")]
        public IActionResult Get()
        {
            var users = _db.Users.ToList();

            var userDTO = users.Select(x => new GetUserDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
            });
            return Ok(userDTO);
        }

        [Authorize]
        [HttpGet]
        [Route("getUser/{idUser}")]
        public async Task<IActionResult> Get(int idUser)
        {
            var user = await _db.Users.Where(u => u.Id == idUser).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { succes = false, message = "No se encontró ningún usuario con este id" });
            }

            var userDTO = new GetUserDTO()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
            };

            return Ok(userDTO);
        }

        [HttpPost]
        [Route("addUser")]
        public async Task<IActionResult> Add([FromBody] AddUserDTO userdto)
        {
            if (userdto == null)
            {
                return BadRequest(new { success = false, message = "Los datos del usuario no son correctos" });
            }
            var existEmail = _db.Users.Where(e => e.Email == userdto.Email);
            if (existEmail.Any())
            {
                return Conflict(new { message = "El email ya está en uso" });
            }

            //Registrar el usuario
            var newUser = new User()
            {
                Name = userdto.Name,
                Email = userdto.Email,
                Password = userdto.Password,
                Rol = userdto.Rol,
            };

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                newUser
            });
        }

        [HttpPut]
        [Route("updateUser")]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (user.Id == 0)
            {
                return BadRequest(new { success = false, message = "El id no existe" });
            }

            var userFound = await _db.Users.Where(u=>u.Id == user.Id).FirstOrDefaultAsync();
            if (userFound == null)
            {
                return NotFound(new { message = "No se encontró el usuario" });
                
            }

            userFound.Name = user.Name;
            userFound.Email = user.Email;
            userFound.Password = user.Password;
            userFound.Rol = user.Rol;
            userFound.CartId = user.CartId;

             _db.Users.Update(userFound);
            await _db.SaveChangesAsync();

            return Ok(new {success = true, message= $"Usuario {userFound.Name} editado" });
        }


        public string GetUserRol(int id)
        {
            if(id == 0)
            {
                return"No existe este id";
            }
            var rol = _db.Users.Where(u=>u.Id == id).Select(r=>r.Rol).ToString();

            if(rol== null)
            {
                return "No se encontro el usuario.";
            }

            return rol;
        }


        [HttpGet]
        [Route("getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            //FindFirst, ayuda a devolver un claim por tipo
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRol = User.FindFirst(ClaimTypes.Role)?.Value;
            //var dssd = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if( currentUserName == null || currentUserRol == null)
            {
                return NotFound(new {success=false, message="Ningún usuario logeado"});
            }

            int id = int.Parse(currentUserName);
            
            if (id == 0)
            {
                return BadRequest(new {success=false, message="Ningún usuario logeado"});
            }
            
            var currentUser = await _db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            var userDTO = new GetUserDTO()
            {
                Id = currentUser.Id,
                Name = currentUser.Name,
                Email = currentUser.Email,
            };
            return Ok(new { success = true, message=$"Usuario logeado: {userDTO.Name}", userDTO, rol=currentUser.Rol });
        }

        [HttpGet]
        public string GetCurrentState(string currentUserName, string sessionId)
        {
            if (!String.IsNullOrEmpty(currentUserName))
            {
                return currentUserName;
            }

            if (!String.IsNullOrEmpty(sessionId))
            {
                return sessionId;
            }

            return "No hay sesion ni usuario logeado";
        }
        
    }
}
