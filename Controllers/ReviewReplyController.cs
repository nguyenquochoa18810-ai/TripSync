using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class ReviewReplyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewReplyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int reviewId)
        {
            ViewBag.ReviewId = reviewId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            int reviewId,
            string replyContent)
        {
            var review =
                _context.Reviews
                .FirstOrDefault(x => x.Id == reviewId);

            if (review == null)
                return NotFound();

            var reply = new ReviewReply
            {
                ReviewId = reviewId,
                ReplyContent = replyContent,
                CreatedAt = DateTime.Now
            };

            _context.ReviewReplies.Add(reply);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Review");
        }
    }
}