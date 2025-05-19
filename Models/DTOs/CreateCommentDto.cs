using System.ComponentModel.DataAnnotations;

namespace SarawakTourismApi.Models.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }
    }
} 