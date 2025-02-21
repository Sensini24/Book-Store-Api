using BookStoreApi.DTO.CommentDTO;
using BookStoreApi.Models;
using BookStoreApi.Sevices;
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

                var comment = await _commentService.AddComment(commentDto);
                return Ok(new { success = true, message="Comentario Guardado", data = comment });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
