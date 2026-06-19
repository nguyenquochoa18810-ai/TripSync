using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class JoinRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JoinRequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Gửi yêu cầu tham gia nhóm
        public async Task<IActionResult> Create(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var isMember = await _context.TripMembers.AnyAsync(x =>
                x.TripGroupId == tripId &&
                x.UserId == user.Id);

            if (isMember)
            {
                TempData["Error"] = "Bạn đã là thành viên của nhóm.";
                return RedirectToAction("MyTrips", "Trip");
            }

            var existedRequest = await _context.JoinRequests.AnyAsync(x =>
                x.TripGroupId == tripId &&
                x.UserId == user.Id &&
                x.Status == "Pending");

            if (existedRequest)
            {
                TempData["Error"] = "Bạn đã gửi yêu cầu tham gia.";
                return RedirectToAction("MyTrips", "Trip");
            }

            var request = new JoinRequest
            {
                TripGroupId = tripId,
                UserId = user.Id,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };

            _context.JoinRequests.Add(request);
            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == tripId);

            if (trip != null)
            {
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = trip.LeaderId,
                        Title = "Yêu cầu tham gia mới",
                        Content = $"{user.FullName} muốn tham gia nhóm {trip.Title}",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
            }

           
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTrips", "Trip");
        }

        // Danh sách yêu cầu của một nhóm
        public IActionResult Manage(int tripId)
        {
            var requests =
                from r in _context.JoinRequests
                join u in _context.Users
                    on r.UserId equals u.Id
                where r.TripGroupId == tripId
                select new
                {
                    r.Id,

                    UserName =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    u.Email,

                    r.RequestDate,

                    r.Status
                };

            ViewBag.Requests = requests.ToList();

            return View();
        }

        // Chấp nhận yêu cầu
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.JoinRequests.FindAsync(id);

            if (request == null)
                return NotFound();

            var exists = await _context.TripMembers.AnyAsync(x =>
                x.TripGroupId == request.TripGroupId &&
                x.UserId == request.UserId);

            if (!exists)
            {
                _context.TripMembers.Add(new TripMember
                {
                    TripGroupId = request.TripGroupId,
                    UserId = request.UserId,
                    JoinedAt = DateTime.Now
                });


                await _context.SaveChangesAsync();

                var budgetExpense = await _context.Expenses
                    .FirstOrDefaultAsync(x =>
                        x.TripGroupId == request.TripGroupId &&
                        x.Title == "Ngân sách chuyến đi");

                if (budgetExpense != null)
                {
                    var memberIds = await _context.TripMembers
                        .Where(x => x.TripGroupId == request.TripGroupId)
                        .Select(x => x.UserId)
                        .ToListAsync();

                    var oldSplits = _context.ExpenseSplits
                        .Where(x => x.ExpenseId == budgetExpense.Id);

                    _context.ExpenseSplits.RemoveRange(oldSplits);

                    decimal splitAmount =
                        budgetExpense.Amount / memberIds.Count;

                    foreach (var memberId in memberIds)
                    {
                        bool isLeader =
                            memberId == budgetExpense.PaidByUserId;

                        _context.ExpenseSplits.Add(
                            new ExpenseSplit
                            {
                                ExpenseId = budgetExpense.Id,
                                UserId = memberId,
                                Amount = isLeader ? 0 : splitAmount,
                                IsPaid = isLeader
                            });
                    }
                }
            }

            request.Status = "Approved";
            _context.Notifications.Add(
            new Notification
            {
                UserId = request.UserId,
                Title = "Yêu cầu được chấp nhận",
                Content = "Bạn đã được chấp nhận vào nhóm.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Manage),
                new { tripId = request.TripGroupId });
        }

        // Từ chối yêu cầu
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.JoinRequests.FindAsync(id);

            if (request == null)
                return NotFound();

            request.Status = "Rejected";
            _context.Notifications.Add(
            new Notification
            {
                UserId = request.UserId,
                Title = "Yêu cầu bị từ chối",
                Content = "Yêu cầu tham gia của bạn đã bị từ chối.",
                IsRead =false,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Manage),
                new { tripId = request.TripGroupId });
        }
    }
}