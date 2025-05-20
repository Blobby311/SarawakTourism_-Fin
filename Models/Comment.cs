using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace SarawakTourismApi.Models
{
    public class Comment : ContentItem
    {
        public int PostId { get; set; }

        [JsonIgnore]
        public Post Post { get; set; } = null!;

        public override string GetContentType()
        {
            return "Comment";
        }

        public override string GetContentSummary()
        {
            return $"Comment on Post {PostId}:\n{base.GetContentSummary()}";
        }
    }
}