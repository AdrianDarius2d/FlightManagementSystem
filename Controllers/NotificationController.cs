using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// Displays and manages in-app notifications (REQ-46 to REQ-53).
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(INotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // REQ-49: view notifications in dashboard
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _notificationService.GetForUserAsync(userId);
            return View(notifications);
        }

        // REQ-50: mark notification as read
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User)!;
            var notifications = await _notificationService.GetForUserAsync(userId);
            foreach (var n in notifications.Where(n => !n.IsRead))
                await _notificationService.MarkAsReadAsync(n.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}
