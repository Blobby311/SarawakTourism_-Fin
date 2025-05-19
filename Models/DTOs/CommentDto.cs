using System;

namespace SarawakTourismApi.Models.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 