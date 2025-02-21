using BookStoreApi.DTO.CommentDTO;
using BookStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DataContext _db;

        public CommentRepository(DataContext db)
        {
            _db = db;
        }

        public async Task<Comment> GetComment(int idComment)
        {
            var comment =  await _db.Comments.Where(c=>c.Id == idComment).FirstOrDefaultAsync();
            return comment;
        }
        public IQueryable<Comment> GetComments()
        {
            var comments = _db.Comments;
            return comments;
        }

        public async Task<Comment> PostComment(Comment comment)
        {
            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            return comment;
        }

        public async Task<Comment> DeleteComments(int idComment)
        {
            var comment = await _db.Comments.Where(c=>c.Id == idComment).FirstOrDefaultAsync();

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            return comment;
        }



        public async Task<Comment> UpdateComment(Comment comment)
        {
            _db.Update(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> ratingComment(int idComment, int commentRating)
        {
            var commentToRate = await _db.Comments.Where(c => c.Id == idComment).FirstOrDefaultAsync();
            commentToRate.Rating = commentRating;

            await _db.SaveChangesAsync();

            return commentToRate;
        }
    }
}
