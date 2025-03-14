﻿using BookStoreApi.Models;

namespace BookStoreApi.DTO.CartDTO
{
    public class AddCartAnonymousDTO
    {
        public int? UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
