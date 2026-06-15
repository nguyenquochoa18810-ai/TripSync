using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    public class VendorRegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public VendorRegisterController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            VendorRegisterViewModel model)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var existingVendor =
                _context.Vendors
                .FirstOrDefault(x =>
                    x.UserId == user.Id);

            if (existingVendor != null)
            {
                TempData["Error"] =
                    "Bạn đã đăng ký Vendor rồi";

                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var vendor = new Vendor
            {
                UserId = user.Id,
                CompanyName = model.CompanyName,
                Description = model.Description,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                IsApproved = false
            };

            _context.Vendors.Add(vendor);

            await _context.SaveChangesAsync();

            var result =
                await _userManager.AddToRoleAsync(
                    user,
                    "Vendor");

            if (!result.Succeeded)
            {
                return Content(
                    string.Join(",",
                    result.Errors.Select(x => x.Description)));
            }

            await _signInManager.SignOutAsync();

            return Redirect("/Identity/Account/Login");
        }
    }
}