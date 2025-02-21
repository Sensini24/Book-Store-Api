namespace BookStoreApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int BookId {  get; set; }
        public virtual Book Book { get; set; }
        public string Content {  get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public int Rating { get; set; }
    }
}
