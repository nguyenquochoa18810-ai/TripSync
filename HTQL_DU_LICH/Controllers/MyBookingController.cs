using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class MyBookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyBookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            var tripIds =
    _context.TripMembers
    .Where(x => x.UserId == user.Id)
    .Select(x => x.TripGroupId)
    .ToList();

            var bookings =
            (
                from b in _context.Bookings

                join s in _context.Services
                    on b.ServiceId equals s.Id

                join t in _context.TripGroups
                    on b.TripGroupId equals t.Id

                where tripIds.Contains(b.TripGroupId)

                select new MyBookingViewModel
                {
                    Id = b.Id,

                    TripName = t.Title,

                    ServiceName = s.Name,

                    BookingDate = b.BookingDate,

                    Status = b.Status,

                    IsReviewed =
                    _context.Reviews.Any(r =>
                        r.BookingId == b.Id &&
                        r.UserId == user.Id)
                }
            ).ToList();

            return View(bookings);
        }
    }
}