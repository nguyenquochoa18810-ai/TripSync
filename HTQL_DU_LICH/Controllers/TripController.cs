using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class TripController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TripController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(MyTrips));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Interests =
                _context.Interests.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var trip = new TripGroup
            {
                Title = model.Title,
                Destination = model.Destination,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                Description = model.Description,
                LeaderId = user.Id
            };
            _context.TripGroups.Add(trip);
            await _context.SaveChangesAsync();

            // ==========================================
            // ĐOẠN CODE LƯU SỞ THÍCH ĐƯỢC THÊM VÀO ĐÂY
            // ==========================================
            if (model.SelectedInterests != null)
            {
                foreach (var interestId in model.SelectedInterests)
                {
                    _context.TripInterests.Add(
                        new TripInterest
                        {
                            TripGroupId = trip.Id,
                            InterestId = interestId
                        });
                }

                await _context.SaveChangesAsync();
            }

            _context.TripMembers.Add(new TripMember
            {
                TripGroupId = trip.Id,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("MyTrips");
        }

        // ==========================================
        // CẬP NHẬT ACTION MYTRIPS (PHÂN LOẠI CHUYẾN ĐI)
        // ==========================================
        public IActionResult MyTrips()
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;

            var model = new MyTripsViewModel();

            model.CreatedTrips =
                _context.TripGroups
                .Where(x => x.LeaderId == userId)
                .ToList();

            model.JoinedTrips =
            (
                from tm in _context.TripMembers
                join t in _context.TripGroups
                    on tm.TripGroupId equals t.Id
                where tm.UserId == userId
                   && t.LeaderId != userId
                select t
            ).ToList();

            model.PendingTrips =
            (
                from jr in _context.JoinRequests
                join t in _context.TripGroups
                    on jr.TripGroupId equals t.Id
                where jr.UserId == userId
                   && jr.Status == "Pending"
                select t
            ).Distinct().ToList();

            return View(model);
        }

        //ACTION DETAIL
        public IActionResult Details(int id)
        {
            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == id);

            if (trip == null)
                return NotFound();

            var leader = _context.Users
                .FirstOrDefault(x => x.Id == trip.LeaderId);

            ViewBag.Leader = leader;

            ViewBag.Members =
            (
                from tm in _context.TripMembers
                join u in _context.Users
                    on tm.UserId equals u.Id
                where tm.TripGroupId == id
                select u
            ).ToList();

            ViewBag.Expenses = _context.Expenses
                .Where(x => x.TripGroupId == id)
                .ToList();
            ViewBag.TotalExpense =
                _context.Expenses
                .Where(x => x.TripGroupId == id)
                .Sum(x => x.Amount);

            ViewBag.AppliedServices =
            (
                from ts in _context.TripServices
                join s in _context.Services
                    on ts.ServiceId equals s.Id
                where ts.TripGroupId == id
                select new
                {
                    Id = ts.Id,

                    ServiceId = s.Id,

                    ServiceName = s.Name,

                    AppliedAt = ts.AppliedAt
                }
            ).ToList();

            // ==========================================
            // KIỂM TRA QUYỀN VÀ TRẠNG THÁI THÀNH VIÊN
            // ==========================================
            var currentUserId =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;

            ViewBag.IsMember =
                _context.TripMembers.Any(x =>
                    x.TripGroupId == id &&
                    x.UserId == currentUserId);

            ViewBag.IsLeader =
                trip.LeaderId == currentUserId;

            ViewBag.Requests =
            (
                from r in _context.JoinRequests
                join u in _context.Users
                    on r.UserId equals u.Id
                where r.TripGroupId == id
                select new
                {
                    r.UserId,

                    UserName =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    Email = u.Email,

                    Status = r.Status
                }
            ).ToList();

            var messages =
(
    from m in _context.GroupMessages
    join u in _context.Users
        on m.UserId equals u.Id
    where m.TripGroupId == id
    orderby m.SentAt
    select new
    {
        UserId = m.UserId,
        UserName =
            string.IsNullOrEmpty(u.FullName)
            ? u.Email
            : u.FullName,

        Message = m.Message,

        Time = m.SentAt
    }
).ToList();

            ViewBag.Messages = messages;

            ViewBag.Services =
                _context.Services
                .ToList();

            ViewBag.ServiceRequests =
(
    from r in _context.ServiceRequests

    join s in _context.Services
        on r.ServiceId equals s.Id

    where r.TripGroupId == id

    select new
    {
        r.Id,
        r.Status,
        ServiceName = s.Name
    }

).ToList();

            ViewBag.ServiceVotes =
                _context.ServiceRequestVotes
                .ToList();

            return View(trip);
        }

        public IActionResult Members(int id)
        {
            var members =
            (
                from tm in _context.TripMembers
                join u in _context.Users
                    on tm.UserId equals u.Id
                where tm.TripGroupId == id
                select new TripMemberViewModel
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio
                }
            ).ToList();

            ViewBag.TripId = id;

            return View(members);
        }

        // ==========================================
        // CẬP NHẬT ACTION JOIN (KIỂM TRA PENDING + GỬI THÔNG BÁO)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var exists =
                _context.JoinRequests.Any(x =>
                    x.TripGroupId == id &&
                    x.UserId == user.Id &&
                    x.Status == "Pending");

            if (exists)
            {
                TempData["Message"] = "Bạn đã gửi yêu cầu tham gia rồi.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // Đã thêm RequestDate = DateTime.Now
            _context.JoinRequests.Add(
                new JoinRequest
                {
                    TripGroupId = id,
                    UserId = user.Id,
                    Status = "Pending",
                    RequestDate = DateTime.Now
                });

            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == id);

            if (trip != null)
            {
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = trip.LeaderId,
                        Title = "Yêu cầu tham gia mới",
                        Content = $"{user.Email} muốn tham gia nhóm {trip.Title}",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // ==========================================
        // CẬP NHẬT ACTION APPROVEREQUEST
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int tripId, string userId)
        {
            var user = await _userManager.GetUserAsync(User);

            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == tripId);

            if (trip == null)
                return NotFound();

            if (trip.LeaderId != user!.Id)
                return Unauthorized();

            var request = _context.JoinRequests
                .FirstOrDefault(x =>
                    x.TripGroupId == tripId &&
                    x.UserId == userId);

            if (request == null)
                return NotFound();

            request.Status = "Approved";

            bool exists = _context.TripMembers.Any(x =>
                x.TripGroupId == tripId &&
                x.UserId == userId);

            if (!exists)
            {
                _context.TripMembers.Add(
                    new TripMember
                    {
                        TripGroupId = tripId,
                        UserId = userId,
                        JoinedAt = DateTime.Now
                    });
            }

            // Thêm thông báo khi duyệt thành công
            _context.Notifications.Add(
                new Notification
                {
                    UserId = userId,
                    Title = "Yêu cầu được chấp nhận",
                    Content = "Bạn đã được chấp nhận vào nhóm.",
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        // ==========================================
        // CẬP NHẬT ACTION REJECTREQUEST
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> RejectRequest(int tripId, string userId)
        {
            var user = await _userManager.GetUserAsync(User);

            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == tripId);

            if (trip == null)
                return NotFound();

            if (trip.LeaderId != user!.Id)
                return Unauthorized();

            var request = _context.JoinRequests
                .FirstOrDefault(x =>
                    x.TripGroupId == tripId &&
                    x.UserId == userId);

            if (request == null)
                return NotFound();

            request.Status = "Rejected";

            // Thêm thông báo khi từ chối yêu cầu
            _context.Notifications.Add(
                new Notification
                {
                    UserId = userId,
                    Title = "Yêu cầu bị từ chối",
                    Content = "Yêu cầu tham gia của bạn đã bị từ chối.",
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        // ==========================================
        // THÊM ĐIỀU KIỆN CHẶN TRƯỞNG NHÓM RỜI NHÓM
        // ==========================================
        public async Task<IActionResult> Leave(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var trip = _context.TripGroups
                .FirstOrDefault(x => x.Id == id);

            if (trip != null && trip.LeaderId == user!.Id)
            {
                TempData["Error"] = "Trưởng nhóm không thể rời nhóm.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var member = _context.TripMembers
                .FirstOrDefault(x =>
                    x.TripGroupId == id &&
                    x.UserId == user!.Id);

            if (member != null)
            {
                _context.TripMembers.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyTrips));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _context.TripGroups.FindAsync(id);

            if (trip == null)
                return NotFound();

            var members = _context.TripMembers
                .Where(x => x.TripGroupId == id);

            _context.TripMembers.RemoveRange(members);

            var requests = _context.JoinRequests
                .Where(x => x.TripGroupId == id);

            _context.JoinRequests.RemoveRange(requests);

            var expenses = _context.Expenses
                .Where(x => x.TripGroupId == id);

            _context.Expenses.RemoveRange(expenses);

            _context.TripGroups.Remove(trip);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyTrips));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(
    int tripId,
    string message)
        {
            var user =
                await _userManager.GetUserAsync(User);

            _context.GroupMessages.Add(
                new GroupMessage
                {
                    TripGroupId = tripId,
                    UserId = user.Id,
                    Message = message
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                new { id = tripId });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyService(
            int tripId,
            int serviceId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            bool exists =
                _context.TripServices.Any(x =>
                    x.TripGroupId == tripId &&
                    x.ServiceId == serviceId);

            if (!exists)
            {
                _context.TripServices.Add(
                    new TripService
                    {
                        TripGroupId = tripId,
                        ServiceId = serviceId,
                        AppliedByUserId = user!.Id
                    });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Details",
                new { id = tripId });
        }

        [HttpPost]
        public IActionResult RemoveService(int id)
        {
            var tripService =
                _context.TripServices
                .FirstOrDefault(x => x.Id == id);

            if (tripService == null)
                return NotFound();

            int tripId = tripService.TripGroupId;

            _context.TripServices.Remove(tripService);

            _context.SaveChanges();

            return RedirectToAction(
                "Details",
                new { id = tripId });
        }


        [HttpPost]
        public async Task<IActionResult>
        ApproveService(int requestId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var request =
                _context.ServiceRequests
                .FirstOrDefault(x =>
                    x.Id == requestId);

            if (request == null)
                return RedirectToAction("MyTrips");

            bool voted =
                _context.ServiceRequestVotes
                .Any(x =>
                    x.ServiceRequestId == requestId
                    && x.UserId == user!.Id);

            if (!voted)
            {
                _context.ServiceRequestVotes.Add(
                    new ServiceRequestVote
                    {
                        ServiceRequestId = requestId,
                        UserId = user.Id,
                        IsApproved = true
                    });

                await _context.SaveChangesAsync();
            }

            var memberCount =
    _context.TripMembers
    .Count(x =>
        x.TripGroupId ==
        request.TripGroupId);

            var yesVotes =
                _context.ServiceRequestVotes
                .Count(x =>
                    x.ServiceRequestId == requestId
                    &&
                    x.IsApproved);

            if (yesVotes >= memberCount
                &&
                request.Status == "Pending")
            {
                var service =
                    _context.Services
                    .FirstOrDefault(x =>
                        x.Id == request.ServiceId);

                if (service != null)
                {
                    _context.Bookings.Add(
                    new Booking
                    {
                        TripGroupId = request.TripGroupId,

                        ServiceId = service.Id,

                        UserId = request.CreatedByUserId,

                        BookingDate = DateTime.Now,

                        Status = "Pending"
                    });
                    _context.Expenses.Add(
                        new Expense
                        {
                            TripGroupId =
                                request.TripGroupId,

                            Title =
                                service.Name,

                            Amount =
                                service.Price,

                            PaidByUserId =
                                request.CreatedByUserId,

                            IsApproved = true,

                            ApprovedAt =
                                DateTime.Now
                        });

                    request.Status =
                        "Approved";

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(
                "Details",
                new { id = request.TripGroupId });
        }

        [HttpPost]
        public async Task<IActionResult>
        RejectService(int requestId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var request =
                _context.ServiceRequests
                .FirstOrDefault(x =>
                    x.Id == requestId);

            if (request == null)
                return RedirectToAction("MyTrips");

            bool voted =
                _context.ServiceRequestVotes
                .Any(x =>
                    x.ServiceRequestId == requestId
                    && x.UserId == user!.Id);

            if (!voted)
            {
                _context.ServiceRequestVotes.Add(
                    new ServiceRequestVote
                    {
                        ServiceRequestId = requestId,
                        UserId = user.Id,
                        IsApproved = false
                    });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Details",
                new { id = request.TripGroupId });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTripService(int tripServiceId)
        {
            var tripService =
                _context.TripServices
                .FirstOrDefault(x => x.Id == tripServiceId);

            if (tripService == null)
                return NotFound();

            var service =
                _context.Services
                .FirstOrDefault(x =>
                    x.Id == tripService.ServiceId);

            if (service == null)
                return NotFound();

            bool exists =
                _context.Expenses.Any(x =>
                    x.TripGroupId == tripService.TripGroupId
                    &&
                    x.Title == service.Name);

            if (!exists)
            {
                var leader =
                    await _userManager.GetUserAsync(User);

                var expense = new Expense
                {
                    TripGroupId = tripService.TripGroupId,
                    PaidByUserId = leader!.Id,
                    Title = service.Name,
                    Amount = service.Price,
                    CreatedAt = DateTime.Now,
                    IsApproved = false
                };

                _context.Expenses.Add(expense);

                await _context.SaveChangesAsync();

                var members =
                    _context.TripMembers
                    .Where(x =>
                        x.TripGroupId ==
                        tripService.TripGroupId)
                    .ToList();

                decimal split =
                    service.Price / members.Count;

                foreach (var member in members)
                {
                    _context.ExpenseSplits.Add(
                        new ExpenseSplit
                        {
                            ExpenseId = expense.Id,
                            UserId = member.UserId,
                            Amount = split,
                            IsPaid = false
                        });
                }

                foreach (var member in members)
                {
                    if (member.UserId != leader.Id)
                    {
                        _context.ExpenseApprovals.Add(
                            new ExpenseApproval
                            {
                                ExpenseId = expense.Id,
                                UserId = member.UserId,
                                IsApproved = false
                            });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                "Details",
                new
                {
                    id = tripService.TripGroupId
                });
        }
    }
}