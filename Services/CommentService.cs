using BookStoreApi.DTO.CommentDTO;
using BookStoreApi.Repositories;
using BookStoreApi.Models;

namespace BookStoreApi.Sevices
{
    public class CommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<GetCommentDTO> GetComment(int idComment)
        {

            if (idComment <= 0)
            {
                throw new ArgumentException("El Id del comentario no puede ser 0 o menor.");
            }

            var comment = await _commentRepository.GetComment(idComment);
            if (comment == null)
            {
                throw new KeyNotFoundException("No se encontró el comentario.");
            }

            var commentDTO = new GetCommentDTO()
            {
                Id = idComment,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Content = comment.Content,
                Created = comment.Created,
                Updated = comment.Updated,
                Rating = comment.Rating,

            };

            return commentDTO;
        }

        public IQueryable<GetCommentDTO> GetComments()
        {
            var comments = _commentRepository.GetComments();
            return comments?.Select(x => new GetCommentDTO()
            {
                Id = x.Id,
                UserId = x.UserId,
                BookId = x.BookId,
                Content = x.Content,
                Created = x.Created,
                Updated = x.Updated,
                Rating = x.Rating,
            }) ?? Enumerable.Empty<GetCommentDTO>().AsQueryable();
        }

        public async Task<GetCommentDTO> AddComment(int userId, AddCommentDTO commentdto)
        {

            var comment = new Comment
            {
                UserId = userId,
                BookId = commentdto.BookId,
                Content = commentdto.Content,
                Created = DateTime.Now,
                Rating = 0
            };
            await _commentRepository.PostComment(comment);
            return new GetCommentDTO()
            {
                Id = comment.Id,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Content = comment.Content,
                Created = comment.Created,
                Updated = comment.Updated,
                Rating = comment.Rating
            };
        }


        public async Task<GetCommentDTO> DeleteComments(int idComment)
        {
            var comment = await _commentRepository.DeleteComments(idComment);

            if (comment == null)
            {
                throw new KeyNotFoundException("El comentario no existe o ya fue eliminado.");
            }

            return new GetCommentDTO()
            {
                Id = comment.Id,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Content = comment.Content,
                Created = comment.Created,
                Updated = comment.Updated,
                Rating = comment.Rating
            };
        }


        public async Task<GetCommentDTO> UpdateComment(EditCommentDTO editcommentdto)
        {

            if (editcommentdto == null)
            {
                throw new NullReferenceException("Se envío un cuerpo nulo.");
            }

            var comment = new Comment()
            {
                Id = editcommentdto.Id,
                UserId = editcommentdto.UserId,
                BookId = editcommentdto.BookId,
                Content = editcommentdto.Content,
                Created = editcommentdto.Created,
                Updated = DateTime.Now,
                Rating = editcommentdto.Rating
            };
            
            var commentd = await _commentRepository.UpdateComment(comment);

            return new GetCommentDTO()
            {
                Id = comment.Id,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Content = comment.Content,
                Created = comment.Created,
                Updated = comment.Updated,
                Rating = comment.Rating
            };

        }

        
        public async Task<GetCommentDTO> ratingComment(int idComment, RateCommentDTO commentdto)
        {
            if (idComment == null)
            {
                throw new ArgumentException("El Id del comentario no puede ser menor a 0.");
            }
            if(commentdto == null)
            {
                throw new ArgumentNullException("El cuerpo no debe ser nulo");
            }

            if(commentdto.Rating <0 || commentdto.Rating > 5)
            {
                throw new ArgumentNullException("Este rating no está permitido");
            }

            var comment = await _commentRepository.ratingComment(idComment, commentdto.Rating);

            return new GetCommentDTO()
            {
                Id = comment.Id,
                UserId = comment.UserId,
                BookId = comment.BookId,
                Content = comment.Content,
                Created = comment.Created,
                Updated = comment.Updated,
                Rating = comment.Rating
            };

        }

    }
}
