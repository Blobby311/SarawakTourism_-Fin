using System;
using System.Collections.Generic;

namespace SarawakTourismApi.Models.DTOs
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Likes { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }
} 