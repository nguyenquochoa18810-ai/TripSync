using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace HTQL_DU_LICH.Controllers
{
    public class ExploreServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExploreServiceController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var services =
                _context.Services
                .Where(x => x.IsActive)
                .ToList();

            ViewBag.Reviews =
                _context.Reviews.ToList();

            ViewBag.Bookings =
                _context.Bookings.ToList();

            return View(services);
        }

        public async Task<IActionResult> Details(int id)

        {
            var service =
                _context.Services
                .FirstOrDefault(x => x.Id == id);

            if (service == null)
                return NotFound();

            var bookingIds =
    _context.Bookings
    .Where(x => x.ServiceId == id)
    .Select(x => x.Id)
    .ToList();

            var reviews =
                _context.Reviews
                .Where(x =>
                    bookingIds.Contains(x.BookingId))
                .ToList();

            ViewBag.AverageRating =
                reviews.Any()
                ? reviews.Average(x => x.Rating)
                : 0;

            ViewBag.TotalReviews =
                reviews.Count;

            ViewBag.Reviews = reviews;
            var user =
                await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewBag.MyTrips =
                (
                from tm in _context.TripMembers
                join t in _context.TripGroups
                on tm.TripGroupId equals t.Id
                where tm.UserId == user.Id
                select t
                ).ToList();
            }

            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> ApplyToTrip(
    int tripId,
    int serviceId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var request =
                new ServiceRequest
                {
                    TripGroupId = tripId,
                    ServiceId = serviceId,
                    CreatedByUserId = user!.Id
                };

            _context.ServiceRequests.Add(request);

            await _context.SaveChangesAsync();

            var service =
    _context.Services
    .FirstOrDefault(x => x.Id == serviceId);

            var members =
                _context.TripMembers
                .Where(x => x.TripGroupId == tripId)
                .ToList();

            foreach (var member in members)
            {
                string content =
                    member.UserId == user.Id
                    ? $"Bạn đã đề xuất dịch vụ '{service?.Name}'"
                    : $"{user.Email} đã đề xuất dịch vụ '{service?.Name}'";

                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = member.UserId,

                        Title = "Yêu cầu dịch vụ mới",

                        Content = content,

                        IsRead = false,

                        CreatedAt = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Trip",
                new { id = tripId });
        }
    }
}