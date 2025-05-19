using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SarawakTourism.Data;
using SarawakTourismApi.Models;
using Microsoft.Extensions.Logging;

namespace SarawakTourismApi.Controllers
{
    public class PostsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostsController> _logger;

        public PostsController(AppDbContext context, ILogger<PostsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all posts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var posts = await _context.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                return Success(posts);
            }
            catch (Exception ex)
            {
                return Error("An error occurred while fetching posts: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        // Get a single post by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return Error("Post not found", HttpStatusCode.NotFound);

            return Success(post);
        }

        // Create a new post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Post post)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Error("Invalid post data");

                if (string.IsNullOrWhiteSpace(post.Username))
                    return Error("Username is required");

                post.CreatedAt = DateTime.UtcNow;
                post.UpdatedAt = DateTime.UtcNow;

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return Success(post, "Post created successfully");
            }
            catch (Exception ex)
            {
                return Error("An error occurred while creating the post: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        // Update an existing post
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Post post)
        {
            if (id != post.Id)
                return Error("Invalid post ID");

            if (!ModelState.IsValid)
                return Error("Invalid post data");

            var existingPost = await _context.Posts.FindAsync(id);
            if (existingPost == null)
                return Error("Post not found", HttpStatusCode.NotFound);

            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Success(existingPost, "Post updated successfully");
        }

        // Delete a post
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return Error("Post not found", HttpStatusCode.NotFound);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Success(new { }, "Post deleted successfully");
        }

        // --- Increment Like Count for a Post ---
        [HttpPost("{id}/like")]
        [Produces("application/json")]
        public async Task<IActionResult> LikePost(int id, [FromQuery] string username)
        {
            try
            {
                _logger.LogInformation("Like request received for post {PostId} from user {Username}", id, username);

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username is missing from request");
                    return Error("Username is required", HttpStatusCode.BadRequest);
                }

                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    _logger.LogWarning("Post {PostId} not found", id);
                    return Error("Post not found", HttpStatusCode.NotFound);
                }

                _logger.LogInformation("Found post: {Title} by {Username}", post.Title, post.Username);

                // Check if user has already liked this post
                var existingLike = await _context.PostLikes
                    .FirstOrDefaultAsync(pl => pl.PostId == id && pl.Username == username);

                if (existingLike != null)
                {
                    _logger.LogWarning("User {Username} has already liked post {PostId}", username, id);
                    return Error("You have already liked this post", HttpStatusCode.BadRequest);
                }

                // Create new like
                var postLike = new PostLike
                {
                    PostId = id,
                    Username = username,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PostLikes.Add(postLike);
                post.Likes++;
                await _context.SaveChangesAsync();

                // Create notification for post owner if liker is not the post owner
                if (post.Username != username)
                {
                    try
                    {
                        _logger.LogInformation("Creating notification for post owner {PostOwner}", post.Username);
                        
                        var notification = new Notification
                        {
                            Username = post.Username,
                            Message = $"{username} liked your post: \"{post.Title}\"",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };

                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Notification created successfully for user {Username}", post.Username);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Failed to create notification for like. Error: {ErrorMessage}", 
                            notifEx.Message);
                        // Don't fail the like if notification fails
                    }
                }
                else
                {
                    _logger.LogInformation("No notification needed - user liked their own post");
                }

                _logger.LogInformation("Successfully added like for post {PostId} by user {Username}", id, username);
                return Success(new { likes = post.Likes }, "Post liked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LikePost: {ErrorMessage}", ex.Message);
                return Error("An error occurred while liking the post: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}