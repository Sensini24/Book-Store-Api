
using BookStoreApi.Models;

namespace BookStoreApi.DTO.CommentDTO
{
    public class EditCommentDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int Rating { get; set; }
    }
}
