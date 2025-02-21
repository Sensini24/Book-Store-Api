using BookStoreApi.Models;

namespace BookStoreApi.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> GetComment(int idComment);
        IQueryable<Comment> GetComments();
        Task<Comment> PostComment(Comment comment);
        Task<Comment> UpdateComment(Comment comment);
        Task<Comment> DeleteComments(int idComment);
        Task<Comment> ratingComment(int idComment, int rating);
    }
}
