namespace BookStoreApi.Models;

public class WishListDetails
{
    public int Id { get; set; }
    public int IdWishList { get; set; }
    public virtual WishList WishList { get; set; }
    public int IdBook { get; set; }
    public virtual Book Book { get; set; }
    public DateTime AddedDate { get; set; }
}