using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SarawakTourismApi.Models
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        
        [JsonIgnore]
        public Post Post { get; set; } = null!;
        
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 