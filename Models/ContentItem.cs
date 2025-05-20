using System.ComponentModel.DataAnnotations;

namespace SarawakTourismApi.Models
{
    public abstract class ContentItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int Likes { get; set; } = 0;

        // Virtual method that can be overridden
        public virtual string GetContentSummary()
        {
            return Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;
        }

        // Abstract method that must be implemented
        public abstract string GetContentType();
    }
} 