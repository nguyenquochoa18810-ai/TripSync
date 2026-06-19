using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public VendorServiceController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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
        public async Task<IActionResult> Create(
    Service model,
    IFormFile? imageFile)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var vendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user!.Id);

            if (vendor == null)
                return Content("Không tìm thấy Vendor");

            if (imageFile != null)
            {
                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(imageFile.FileName);

                string folder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "services");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath =
                    Path.Combine(folder, fileName);

                using (var stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImageUrl =
                    "/uploads/services/" + fileName;
            }

            model.VendorId = vendor.Id;

            _context.Services.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


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
        public async Task<IActionResult> Edit(
            Service model,
            IFormFile? imageFile)
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

            if (imageFile != null)
            {
                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(imageFile.FileName);

                string folder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "services");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath =
                    Path.Combine(folder, fileName);

                using (var stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                service.ImageUrl =
                    "/uploads/services/" + fileName;
            }

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