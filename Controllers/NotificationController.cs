using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var notifications = _context.Notifications
                .Where(x => x.UserId == user!.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(notifications);
        }
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification =
                await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var notifications =
                _context.Notifications
                    .Where(x =>
                        x.UserId == user.Id &&
                        !x.IsRead)
                    .ToList();

            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        

    }
}