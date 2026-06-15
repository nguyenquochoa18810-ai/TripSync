using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorBookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorBookingController(
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

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            if (vendor == null)
                return Content("Không tìm thấy Vendor");

            var serviceIds =
                _context.Services
                .Where(x =>
                    x.VendorId == vendor.Id)
                .Select(x => x.Id)
                .ToList();

            var bookings =
            (
                from b in _context.Bookings

                join s in _context.Services
                    on b.ServiceId equals s.Id

                join u in _context.Users
                    on b.UserId equals u.Id

                where serviceIds.Contains(b.ServiceId)

                select new VendorBookingViewModel
                {
                    Id = b.Id,

                    CustomerName =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email!
                        : u.FullName,

                    ServiceName = s.Name,

                    BookingDate = b.BookingDate,

                    Status = b.Status
                }
            ).ToList();

            return View(bookings);
        }
        public async Task<IActionResult> Confirm(int id)
        {
            var booking =
                await _context.Bookings.FindAsync(id);

            if (booking == null)
                return Content("Booking không tồn tại");

            booking.Status = "Confirmed";

            await _context.SaveChangesAsync();

            return Content(
                $"Booking {booking.Id} đã chuyển thành {booking.Status}"
            );
        }
        public async Task<IActionResult>
        Complete(int id)
        {
            var booking =
                await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            booking.Status = "Completed";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }




}