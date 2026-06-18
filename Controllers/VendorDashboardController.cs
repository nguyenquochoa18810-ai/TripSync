using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public VendorDashboardController(
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

            if (user == null)
                return RedirectToAction(
                    "Login",
                    "Auth");

            var vendor =
    _context.Vendors
    .FirstOrDefault(x =>
        x.UserId == user.Id);

            if (vendor == null)
            {
                return RedirectToAction(
                    "Create",
                    "VendorRegister");
            }

            var serviceIds =
                _context.Services
                .Where(x =>
                    x.VendorId == vendor.Id)
                .Select(x => x.Id)
                .ToList();

            var bookingIds =
                _context.Bookings
                .Where(x =>
                    serviceIds.Contains(
                        x.ServiceId))
                .Select(x => x.Id)
                .ToList();


            if (vendor == null)
                return Content(
                    "Bạn chưa có hồ sơ Vendor");

            var reviews =
    _context.Reviews
    .Where(x => x.VendorId == vendor.Id)
    .ToList();

            var model =
                new VendorDashboardViewModel
                {
                    TotalServices =
                    _context.Services
                    .Count(x => x.VendorId == vendor.Id),

                    TotalBookings =
                    bookingIds.Count,

                    PendingBookings =
                    _context.Bookings.Count(x =>
                        bookingIds.Contains(x.Id)
                        && x.Status == "Pending"),

                    ConfirmedBookings =
                    _context.Bookings.Count(x =>
                        bookingIds.Contains(x.Id)
                        && x.Status == "Confirmed"),

                    CompletedBookings =
                    _context.Bookings.Count(x =>
                        bookingIds.Contains(x.Id)
                        && x.Status == "Completed"),

                    CancelledBookings =
                    _context.Bookings.Count(x =>
                        bookingIds.Contains(x.Id)
                        && x.Status == "Cancelled"),

                    TotalReviews =
                    reviews.Count,

                    AverageRating =
                    reviews.Any()
                        ? reviews.Average(x => x.Rating)
                        : 0
                };

            return View(model);
        }
    }
}