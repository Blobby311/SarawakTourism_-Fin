using System.Text.Json.Serialization;

namespace SarawakTourismApi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty; // Default to an empty string
        public string Username { get; set; } = string.Empty; // Default to an empty string
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } // To track if the notification has been read
    }
}