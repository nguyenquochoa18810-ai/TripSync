using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class MatchingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var userInterests = _context.UserInterests
                .Where(x => x.UserId == user!.Id)
                .Select(x => x.InterestId)
                .ToList();

            var results = new List<MatchResultViewModel>();

            foreach (var trip in _context.TripGroups)
            {
                // ==========================================
                // KIỂM TRA TRẠNG THÁI NGƯỜI DÙNG VỚI NHÓM
                // ==========================================
                string status = "Chưa tham gia";

                if (trip.LeaderId == user!.Id)
                {
                    status = "Trưởng nhóm";
                }
                else if (_context.TripMembers.Any(x =>
                         x.TripGroupId == trip.Id &&
                         x.UserId == user.Id))
                {
                    status = "Đã tham gia";
                }
                else if (_context.JoinRequests.Any(x =>
                         x.TripGroupId == trip.Id &&
                         x.UserId == user.Id &&
                         x.Status == "Pending"))
                {
                    status = "Chờ duyệt";
                }

                // ==========================================
                // TÍNH ĐIỂM PHÙ HỢP
                // ==========================================
                var tripInterests = _context.TripInterests
                    .Where(x => x.TripGroupId == trip.Id)
                    .Select(x => x.InterestId)
                    .ToList();

                int common = tripInterests
                    .Intersect(userInterests)
                    .Count();

                int score = tripInterests.Count == 0
                    ? 0
                    : (common * 100) / tripInterests.Count;

                // ==========================================
                // ĐẾM SỐ LƯỢNG THÀNH VIÊN
                // ==========================================
                int memberCount =
                    _context.TripMembers
                    .Count(x => x.TripGroupId == trip.Id);

                // ==========================================
                // LẤY DANH SÁCH TÊN SỞ THÍCH CỦA CHUYẾN ĐI
                // ==========================================
                var interestNames =
                (
                    from ti in _context.TripInterests
                    join i in _context.Interests
                        on ti.InterestId equals i.Id
                    where ti.TripGroupId == trip.Id
                    select i.Name
                ).ToList();

                // ==========================================
                // CẬP NHẬT KẾT QUẢ ADD VÀO LIST
                // ==========================================
                results.Add(new MatchResultViewModel
                {
                    TripId = trip.Id,
                    TripTitle = trip.Title,
                    MatchPercent = score,
                    Status = status,
                    MemberCount = memberCount,
                    Interests = interestNames,

                    CommonInterestCount = common,
                    TotalTripInterestCount = tripInterests.Count
                });
            }

            results = results
                .OrderByDescending(x => x.MatchPercent)
                .ToList();

            return View(results);
        }
    }
}