using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class BookingController : Controller
    {
        // Kiểm tra Constructor: ĐẠT chuẩn
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(
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
                var bookings =
                    _context.Bookings.ToList();

                return View(bookings);
            }

            var vendorServiceIds =
                _context.Services
                .Where(x => x.VendorId == vendor.Id)
                .Select(x => x.Id)
                .ToList();

            var bookingsOfVendor =
                _context.Bookings
                .Where(x =>
                    vendorServiceIds.Contains(
                        x.ServiceId))
                .ToList();

            ViewBag.Trips =
                _context.TripGroups.ToList();

            ViewBag.Services =
                _context.Services.ToList();
            return View(bookingsOfVendor);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Trips =
                _context.TripGroups.ToList();

            ViewBag.Services =
                _context.Services.ToList();

            return View();
        }

        // Kiểm tra Action POST Create: ĐẠT chuẩn logic bảo mật UserId
        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Thêm dấu ! để khẳng định user chắc chắn không null sau bước check phía trên
            booking.UserId = user!.Id;
            booking.BookingDate = DateTime.Now;
            booking.Status = "Pending";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            booking.Status = "Confirmed";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            booking.Status = "Completed";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ApplyService(
    int tripId,
    int serviceId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(
                    "Login",
                    "Auth");

            var booking = new Booking
            {
                TripGroupId = tripId,

                ServiceId = serviceId,

                UserId = user.Id,

                BookingDate = DateTime.Now,

                Status = "Pending"
            };

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Trip",
                new { id = tripId });
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var booking =
                await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}