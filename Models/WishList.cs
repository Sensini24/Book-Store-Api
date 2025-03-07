namespace BookStoreApi.Models;

public class WishList
{
    public int Id { get; set; }
    public int IdUser { get; set; }
    public virtual User User { get; set; }
    public DateTime DateCreated { get; set; }
    public ICollection<WishListDetails> WishListDetails { get; set; }
}