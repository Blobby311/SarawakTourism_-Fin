using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SarawakTourism.Data;
using SarawakTourismApi.Models;
using SarawakTourismApi.Models.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SarawakTourismApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(AppDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Comments
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetComments()
        {
            try
            {
                var comments = await _context.Comments
                    .Include(c => c.Post)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Success<List<Comment>>(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments");
                return Error("Failed to fetch comments");
            }
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComment(int id)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Post)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comment == null)
                {
                    return Error("Comment not found", HttpStatusCode.NotFound);
                }

                return Success<Comment>(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment {CommentId}", id);
                return Error("Failed to fetch comment");
            }
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto commentDto)
        {
            try
            {
                _logger.LogInformation("Creating new comment: {@CommentDto}", commentDto);

                if (commentDto == null)
                {
                    _logger.LogWarning("Comment data is null");
                    return Error("No comment data provided", HttpStatusCode.BadRequest);
                }

                if (string.IsNullOrEmpty(commentDto.Username))
                {
                    _logger.LogWarning("Username is missing from comment");
                    return Error("Username is required", HttpStatusCode.BadRequest);
                }

                if (string.IsNullOrEmpty(commentDto.Content))
                {
                    _logger.LogWarning("Content is missing from comment");
                    return Error("Comment content is required", HttpStatusCode.BadRequest);
                }

                if (commentDto.PostId <= 0)
                {
                    _logger.LogWarning("Invalid PostId: {PostId}", commentDto.PostId);
                    return Error("Invalid post ID", HttpStatusCode.BadRequest);
                }

                var post = await _context.Posts.FindAsync(commentDto.PostId);
                if (post == null)
                {
                    _logger.LogWarning("Post not found with ID: {PostId}", commentDto.PostId);
                    return Error("Post not found", HttpStatusCode.NotFound);
                }

                var comment = new Comment
                {
                    Content = commentDto.Content,
                    Username = commentDto.Username,
                    PostId = commentDto.PostId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Comment created successfully with ID: {CommentId}", comment.Id);

                // Create notification for post owner if commenter is not the post owner
                if (post.Username != comment.Username)
                {
                    try
                    {
                        _logger.LogInformation("Creating notification for post owner {PostOwner} from commenter {Commenter}", 
                            post.Username, comment.Username);
                            
                        var notification = new Notification
                        {
                            Username = post.Username,
                            Message = $"{comment.Username} commented on your post: \"{post.Title}\"",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        _logger.LogInformation("Adding notification to database: {@Notification}", notification);
                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Notification created successfully for user {Username} with ID {NotificationId}", 
                            post.Username, notification.Id);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Failed to create notification for comment. Error: {ErrorMessage}", 
                            notifEx.Message);
                        // Don't fail the comment if notification fails
                    }
                }
                else
                {
                    _logger.LogInformation("Skipping notification creation - commenter {Commenter} is the post owner", 
                        comment.Username);
                }

                // Return a simplified response without circular references
                var response = new
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    Username = comment.Username,
                    PostId = comment.PostId,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt
                };

                return Success<object>(response, "Comment added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment: {ErrorMessage}", ex.Message);
                return Error("An error occurred while creating the comment: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        // PUT: api/Comments/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Comment comment)
        {
            try
            {
                if (id != comment.Id)
                {
                    return Error("Invalid comment ID");
                }

                var existingComment = await _context.Comments.FindAsync(id);
                if (existingComment == null)
                {
                    return Error("Comment not found", HttpStatusCode.NotFound);
                }

                existingComment.Content = comment.Content;
                existingComment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Success<Comment>(existingComment, "Comment updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", id);
                return Error("Failed to update comment");
            }
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return Error("Comment not found", HttpStatusCode.NotFound);
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                return Success<object>(new { }, "Comment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return Error("Failed to delete comment");
            }
        }

        // GET: api/Comments/post/5
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsForPost(int postId)
        {
            try
            {
                _logger.LogInformation("Fetching comments for post {PostId}", postId);

                var comments = await _context.Comments
                    .Where(c => c.PostId == postId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Username = c.Username,
                        PostId = c.PostId,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} comments for post {PostId}", comments.Count, postId);
                return Success<object>(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments for post {PostId}", postId);
                return Error("An error occurred while fetching comments: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}