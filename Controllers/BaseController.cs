using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using SarawakTourism.Data;
using SarawakTourismApi.Models;
using SarawakTourismApi.Models.DTOs;

namespace SarawakTourismApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        protected IActionResult Success<T>(T data = default, string message = "Success")
        {
            try
            {
                var response = new
                {
                    success = true,
                    status = 200,
                    message = message,
                    data = data
                };

                return new JsonResult(response, _jsonOptions)
                {
                    StatusCode = 200,
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                return Error($"Error serializing response: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        protected IActionResult Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            try
            {
                var response = new
                {
                    success = false,
                    status = (int)statusCode,
                    message = message,
                    data = (object)null
                };

                return new JsonResult(response, _jsonOptions)
                {
                    StatusCode = (int)statusCode,
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = "Error serializing error response" })
                {
                    StatusCode = 500,
                    ContentType = "application/json"
                };
            }
        }

        protected async Task CreateNotification(AppDbContext context, string username, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("CreateNotification: Username is empty, skipping notification");
                    return;
                }

                Console.WriteLine($"Creating notification for user: {username}");
                Console.WriteLine($"Notification message: {message}");

                var notification = new Notification
                {
                    Username = username,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
                Console.WriteLine($"Successfully created notification with ID: {notification.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to be handled by the caller
            }
        }

        protected CommentDto MapToCommentDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Username = comment.Username,
                PostId = comment.PostId,
                CreatedAt = comment.CreatedAt
            };
        }

        protected PostDto MapToPostDto(Post post, IEnumerable<Comment> comments = null)
        {
            var dto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Username = post.Username,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                Likes = post.Likes
            };

            if (comments != null)
            {
                dto.Comments = comments.Select(MapToCommentDto).ToList();
            }

            return dto;
        }
    }
}