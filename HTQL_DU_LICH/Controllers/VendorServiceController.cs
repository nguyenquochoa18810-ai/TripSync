using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendorServiceController(
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

            var services =
                _context.Services
                .Where(x =>
                    x.VendorId == vendor.Id)
                .ToList();

            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service model)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            if (vendor == null)
                return Content("Không tìm thấy Vendor");

            model.VendorId = vendor.Id;

            model.Name ??= "";
            model.ServiceType ??= "";
            model.Description ??= "";

            _context.Services.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            if (vendor == null)
                return Content("Không tìm thấy Vendor");

            var service =
                _context.Services
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.VendorId == vendor.Id);

            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _userManager.GetUserAsync(User);

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            if (vendor == null)
                return Content("Không tìm thấy Vendor");

            var service =
                _context.Services
                .FirstOrDefault(x =>
                    x.Id == model.Id &&
                    x.VendorId == vendor.Id);

            if (service == null)
                return NotFound();

            service.Name =
                model.Name?.Trim() ?? "";

            service.ServiceType =
                model.ServiceType?.Trim() ?? "";

            service.Description =
                model.Description?.Trim() ?? "";

            service.Price =
                model.Price;

            service.IsActive =
                model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật dịch vụ thành công";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            var service =
                _context.Services
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.VendorId == vendor!.Id);

            if (service == null)
                return NotFound();

            _context.Services.Remove(service);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}