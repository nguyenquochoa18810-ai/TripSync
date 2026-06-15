using HTQL_DU_LICH.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? keyword)
        {
            var trips = _context.TripGroups.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                trips = trips.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.Destination.Contains(keyword));
            }

            return View(trips.ToList());
        }
    }
}