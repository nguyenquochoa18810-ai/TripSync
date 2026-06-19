using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (await _userManager.IsInRoleAsync(user, "Vendor"))
            {
                return RedirectToAction(
                    "Index",
                    "VendorDashboard");
            }

            var myTripIds =
                _context.TripMembers
                    .Where(x => x.UserId == user.Id)
                    .Select(x => x.TripGroupId)
                    .ToList();

            var latestTrip =
                _context.TripGroups
                    .Where(x => myTripIds.Contains(x.Id))
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();

            var latestBooking =
                _context.Bookings
                    .Where(x => x.UserId == user.Id)
                    .OrderByDescending(x => x.BookingDate)
                    .FirstOrDefault();

            var latestService =
                latestBooking == null
                    ? null
                    : _context.Services
                        .FirstOrDefault(x => x.Id == latestBooking.ServiceId);

            var tripIds =
    _context.TripMembers
    .Where(x => x.UserId == user.Id)
    .Select(x => x.TripGroupId)
    .ToList();

            var myDebt =
            (
                from split in _context.ExpenseSplits

                join expense in _context.Expenses
                    on split.ExpenseId equals expense.Id

                where split.UserId == user.Id

                    && expense.PaidByUserId != user.Id

                    && expense.IsApproved

                    && !split.IsPaid

                    && tripIds.Contains(expense.TripGroupId)

                select split.Amount

            ).Sum();

            var topServices =
(
    from s in _context.Services

    select new TopServiceViewModel
    {
        ServiceId = s.Id,

        ServiceName = s.Name,

        Price = s.Price,

        AverageRating =
        (
            from r in _context.Reviews

            join b in _context.Bookings
                on r.BookingId equals b.Id

            where b.ServiceId == s.Id

            select (double?)r.Rating
        ).Average() ?? 0,

        TotalReviews =
        (
            from r in _context.Reviews

            join b in _context.Bookings
                on r.BookingId equals b.Id

            where b.ServiceId == s.Id

            select r
        ).Count()
    }
)
.Where(x => x.TotalReviews > 0)
.OrderByDescending(x => x.AverageRating)
.ThenByDescending(x => x.TotalReviews)
.Take(5)
.ToList();

            ViewBag.TopServices = topServices;

            ViewBag.PendingServices =
            (
                from r in _context.ServiceRequests
                where r.Status == "Pending"
                select r
            ).Count();

            var model = new DashboardViewModel
            {
                TotalTrips = _context.TripGroups.Count(),

                TotalMembers = _context.TripMembers.Count(),

                TotalNotifications = _context.Notifications
                    .Count(x => x.UserId == user.Id && !x.IsRead),

                PendingRequests = _context.JoinRequests
                    .Count(x => x.Status == "Pending"),

                TotalExpense = _context.Expenses
                    .Sum(x => (decimal?)x.Amount) ?? 0,

                // ==========================================
                // ĐOẠN GÁN CÁC THUỘC TÍNH MỚI THEO YÊU CẦU
                // ==========================================
                LatestTripName = latestTrip?.Title, 

                LatestBookingService = latestService?.Name,

                LatestBookingStatus = latestBooking?.Status,

                MyDebt = myDebt
            };

            return View(model);
        }
    }
}