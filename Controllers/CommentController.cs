using System.Security.Claims;
using BookStoreApi.DTO.CommentDTO;
using BookStoreApi.Models;
using BookStoreApi.Sevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : Controller
    {
        private readonly DataContext _db;
        private readonly CommentService _commentService;

        public CommentController(DataContext db, CommentService commentService)
        {
            _db = db;
            _commentService = commentService;
        }

        [HttpGet]
        [Route("getComment")]
        public IActionResult GetComment(int idComment)
        {
            try
            {
                var comment = _commentService.GetComment(idComment);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getComments")]
        public IActionResult GetComments()
        {
            try
            {
                var comments = _commentService.GetComments();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpPost]
        [Route("addComment")]
        public async Task<IActionResult> PostComment([FromBody] AddCommentDTO commentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var sessionId = Request.Cookies["SessionId"];
                var currentUserName =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
                bool isUserLoggedIn = !string.IsNullOrEmpty(currentUserName);
                bool hasSession = !string.IsNullOrEmpty(sessionId);

                if (!isUserLoggedIn)
                {
                    return Unauthorized(new { success = false, message="Tienes que registrarte para poder comentar" });
                }
                
                int userId = int.Parse(currentUserName);
                var commentTotal = new Comment
                {
                    UserId = userId,
                    BookId = commentDto.BookId,
                    Content = commentDto.Content
                };

                var comment = await _commentService.AddComment(userId, commentDto);
                return Ok(new { success = true, message="Comentario Guardado", data = comment });
                
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            
            return BadRequest();
        }

        [HttpDelete]
        [Route("deleteComment/{idComment}")]
        public async Task<IActionResult> DeleteComments(int idComment)
        {
            try
            {
                var comment = await _commentService.DeleteComments(idComment);

                return Ok(new { success = true, message = "Comentario eliminado", data = comment });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("editComment")]
        public async Task<IActionResult> UpdateComment([FromBody] EditCommentDTO editCommentDTO)
        {
            try
            {
                var comment = await _commentService.UpdateComment(editCommentDTO);

                return Ok(new { success = true, message = "Comentario editado", data = comment });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("rateComment/{idComment}")]
        public async Task<IActionResult> ratingComment(int idComment, [FromBody] RateCommentDTO commentdto)
        {
            try
            {
                var comment = await _commentService.ratingComment(idComment, commentdto);

                return Ok(new { success = true, message = "Nueva calificación hecha", data = comment });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
