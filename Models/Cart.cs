
namespace BookStoreApi.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<CartItem> CartItems { get; set; }
        //public decimal Total => Items?.Sum(i => i.Quantity * i.Book.Price) ?? 0;

    }
}

//CREATE TABLE ShoppingCart (
//    CartID INT PRIMARY KEY AUTO_INCREMENT,
//    SessionID VARCHAR(255) NOT NULL,
//    UserID INT,
//    FechaCreación DATETIME DEFAULT CURRENT_TIMESTAMP,
//    FOREIGN KEY (UserID) REFERENCES User(UserID)
//);

//CREATE TABLE CartItem (
//    CartItemID INT PRIMARY KEY AUTO_INCREMENT,
//    CartID INT,
//    BookID INT,
//    Cantidad INT,
//    PrecioUnitario DECIMAL(10, 2),
//    FOREIGN KEY (CartID) REFERENCES ShoppingCart(CartID),
//    FOREIGN KEY (BookID) REFERENCES Book(BookID)
//);
