
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Models
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Comment> Comments { get; set; }
        //public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderDetail> OrderDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(p => p.Genre)
                .WithMany(p => p.Books)
                .HasForeignKey(p => p.IdGenre)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .Property(o => o.Price)
                .HasColumnType("decimal(18, 2)");


            modelBuilder.Entity<CartItem>()
                .HasOne(p=>p.Cart)
                .WithMany(p=>p.CartItems)
                .HasForeignKey(p => p.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(p => p.Book)
                .WithMany(p => p.CartItems)
                .HasForeignKey(p => p.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .Property(o => o.UnitPrice)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Cart>()
                .HasOne(p => p.User)
                .WithOne(p => p.Cart)
                .HasForeignKey<Cart>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(e=>e.User)
                .WithMany(p=>p.Comments)
                .HasForeignKey(o=>o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(e => e.Book)
                .WithMany(p => p.Comments)
                .HasForeignKey(o => o.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
