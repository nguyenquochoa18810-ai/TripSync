using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Vendor")]
public class VendorProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VendorProfileController(
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

        return View(vendor);
    }

    [HttpPost]
    public async Task<IActionResult>
Update(Vendor model)
    {
        var vendor =
            await _context.Vendors
            .FindAsync(model.Id);

        if (vendor == null)
            return NotFound();

        vendor.CompanyName =
            model.CompanyName;

        vendor.Description =
            model.Description;

        vendor.Phone =
            model.Phone;

        vendor.Email =
            model.Email;

        vendor.Address =
            model.Address;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Cập nhật hồ sơ thành công";

        return RedirectToAction(nameof(Index));
    }

}