using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            var userId =
                _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                ViewBag.UnreadCount =
                    _context.Notifications.Count(x =>
                        x.UserId == userId &&
                        !x.IsRead);
            }

            base.OnActionExecuting(context);
        }
    }
}