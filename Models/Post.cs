using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SarawakTourismApi.Models
{
    public class Post : ContentItem
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public override string GetContentType()
        {
            return "Post";
        }

        public override string GetContentSummary()
        {
            return $"Title: {Title}\n{base.GetContentSummary()}";
        }
    }
}