using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(
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

            List<Review> reviews;

            if (User.IsInRole("Vendor"))
            {
                var vendor =
                    _context.Vendors
                    .FirstOrDefault(x =>
                        x.UserId == user!.Id);

                if (vendor == null)
                {
                    reviews = new List<Review>();
                }
                else
                {
                    reviews =
                        _context.Reviews
                        .Where(x =>
                            x.VendorId == vendor.Id)
                        .ToList();
                }
            }
            else
            {
                reviews =
                    _context.Reviews
                    .Where(x =>
                        x.UserId == user!.Id)
                    .ToList();
            }

            ViewBag.Users =
                _context.Users.ToList();

            ViewBag.Bookings =
                _context.Bookings.ToList();

            ViewBag.Services =
                _context.Services.ToList();

            ViewBag.Replies =
                _context.ReviewReplies.ToList();

            return View(reviews);
        }

        public IActionResult Summary(int serviceId)
        {
            var bookingIds =
                _context.Bookings
                .Where(x => x.ServiceId == serviceId)
                .Select(x => x.Id)
                .ToList();

            var reviews =
                _context.Reviews
                .Where(x =>
                    bookingIds.Contains(x.BookingId))
                .ToList();

            var service =
                _context.Services
                .FirstOrDefault(x =>
                    x.Id == serviceId);

            var model =
                new ServiceReviewSummaryViewModel
                {
                    ServiceName =
                        service?.Name ?? "",

                    TotalReviews =
                        reviews.Count,

                    AverageRating =
                        reviews.Any()
                        ? reviews.Average(x => x.Rating)
                        : 0,

                    FiveStar =
                        reviews.Count(x => x.Rating == 5),

                    FourStar =
                        reviews.Count(x => x.Rating == 4),

                    ThreeStar =
                        reviews.Count(x => x.Rating == 3),

                    TwoStar =
                        reviews.Count(x => x.Rating == 2),

                    OneStar =
                        reviews.Count(x => x.Rating == 1)
                };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int bookingId)
        {
            var booking =
                _context.Bookings
                .FirstOrDefault(x =>
                    x.Id == bookingId);

            if (booking == null)
                return NotFound();

            var user =
                _userManager.GetUserAsync(User).Result;

            bool isMember =
                _context.TripMembers.Any(x =>
                    x.TripGroupId == booking.TripGroupId &&
                    x.UserId == user!.Id);

            if (!isMember)
            {
                TempData["Error"] =
                    "Bạn không thuộc nhóm sử dụng dịch vụ này.";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            if (booking.Status != "Completed")
            {
                TempData["Error"] =
                    "Bạn chỉ có thể đánh giá sau khi dịch vụ hoàn thành.";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            return View(new Review
            {
                BookingId = bookingId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Review review)
        {
            var user = await _userManager.GetUserAsync(User);

            bool reviewed =
                _context.Reviews.Any(x =>
                    x.BookingId == review.BookingId
                    && x.UserId == user!.Id);

            if (reviewed)
            {
                TempData["Error"] =
                    "Bạn đã đánh giá dịch vụ này rồi";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            if (user == null)
                return RedirectToAction("Login", "Auth");

            review.UserId = user.Id;

            
            var booking =
                _context.Bookings
                .FirstOrDefault(x =>
                    x.Id == review.BookingId);

            if (booking == null)
            {
                TempData["Error"] =
                    "Không tìm thấy booking.";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            bool isMember =
                _context.TripMembers.Any(x =>
                    x.TripGroupId == booking.TripGroupId &&
                    x.UserId == user.Id);

            if (!isMember)
            {
                TempData["Error"] =
                    "Bạn không thuộc nhóm sử dụng dịch vụ này.";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            if (booking.Status != "Completed")
            {
                TempData["Error"] =
                    "Bạn chỉ có thể đánh giá sau khi dịch vụ hoàn thành.";

                return RedirectToAction(
                    "Index",
                    "MyBooking");
            }

            if (booking != null)
            {
                var service =
                    _context.Services
                    .FirstOrDefault(x =>
                        x.Id == booking.ServiceId);

                if (service != null)
                {
                    review.VendorId =
                        service.VendorId;
                }
            }

            review.CreatedAt = DateTime.Now;
            
            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đánh giá đã được gửi thành công.";

            return RedirectToAction(
                "Index",
                "MyBooking");
        }
    }
}