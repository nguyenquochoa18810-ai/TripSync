using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class MyReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyReviewController(
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

            var reviews =
                _context.Reviews
                .Where(x => x.UserId == user!.Id)
                .ToList();

            return View(reviews);
        }
    }
}