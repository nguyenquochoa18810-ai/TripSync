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
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var existingVendor = _context.Vendors
                .FirstOrDefault(x => x.UserId == user.Id);

            if (existingVendor != null)
            {
                TempData["Error"] = "Bạn đã đăng ký nhà cung cấp rồi.";

                return RedirectToAction(
                    "Index",
                    "VendorProfile");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            VendorRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
                    "Bạn đã đăng ký nhà cung cấp rồi.";

                return RedirectToAction(
                    "Index",
                    "VendorProfile");
            }

            var vendor = new Vendor
            {
                UserId = user.Id,
                CompanyName = model.CompanyName?.Trim() ?? "",
                Description = model.Description?.Trim() ?? "",
                Phone = model.Phone?.Trim() ?? "",
                Email = model.Email?.Trim() ?? "",
                Address = model.Address?.Trim() ?? "",
                IsApproved = false
            };

            try
            {
                _context.Vendors.Add(vendor);

                await _context.SaveChangesAsync();

                if (!await _userManager.IsInRoleAsync(user, "Vendor"))
                {
                    var result =
                        await _userManager.AddToRoleAsync(
                            user,
                            "Vendor");

                    if (!result.Succeeded)
                    {
                        TempData["Error"] =
                            string.Join(
                                ", ",
                                result.Errors.Select(x => x.Description));
                    }
                }

                TempData["Success"] =
                    "Đăng ký nhà cung cấp thành công.";

                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction(
                    "Index",
                    "VendorProfile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra: " + ex.Message);

                return View(model);
            }
        }
    }
}