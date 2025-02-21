﻿using BookStoreApi.Models;

namespace BookStoreApi.DTO.CommentDTO
{
    public class AddCommentDTO
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string Content { get; set; }
    }
}
