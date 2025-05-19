using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SarawakTourism.Data;
using SarawakTourismApi.Models;
using System.Text.Json;

namespace SarawakTourismApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(AppDbContext context, ILogger<NotificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(string username)
        {
            try
            {
                _logger.LogInformation("Fetching notifications for user: {Username}", username);

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username is missing from request");
                    return Success(new List<Notification>(), "No username provided");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.Username == username && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} notifications for user {Username}. Notifications: {@Notifications}", 
                    notifications.Count, username, notifications);

                // Ensure we always return a valid JSON response
                return Success(notifications ?? new List<Notification>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user {Username}", username);
                // Return an empty list instead of an error to prevent JSON parsing issues
                return Success(new List<Notification>(), "Error occurred but returning empty list");
            }
        }

        // Debug endpoint to check all notifications
        [HttpGet("debug/all")]
        public async Task<IActionResult> GetAllNotifications()
        {
            try
            {
                _logger.LogInformation("Fetching all notifications from database");
                
                var allNotifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} total notifications in database. Notifications: {@Notifications}", 
                    allNotifications.Count, allNotifications);

                return Success<List<Notification>>(allNotifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all notifications: {ErrorMessage}", ex.Message);
                return Error("An error occurred while fetching all notifications: " + ex.Message);
            }
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkNotificationsAsRead(string username)
        {
            try
            {
                _logger.LogInformation("Marking notifications as read for user: {Username}", username);

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Username is missing from request");
                    return Error("Username is required", HttpStatusCode.BadRequest);
                }

                var notifications = await _context.Notifications
                    .Where(n => n.Username == username && !n.IsRead)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} unread notifications to mark as read: {@Notifications}", 
                    notifications.Count, notifications);

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
                return Success(new { }, "Notifications marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read for user {Username}", username);
                return Error("An error occurred while marking notifications as read: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}