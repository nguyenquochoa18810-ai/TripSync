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


            // Thêm trưởng nhóm vào thành viên trước
            _context.TripMembers.Add(
    new TripMember
    {
        TripGroupId = trip.Id,
        UserId = user.Id,
        JoinedAt = DateTime.Now
    });



            await _context.SaveChangesAsync();



            // Tạo ngân sách chuyến đi
            var budgetExpense = new Expense
            {
                TripGroupId = trip.Id,
                Title = "Ngân sách chuyến đi",
                Amount = model.Budget,
                PaidByUserId = user.Id,
                IsApproved = true,
                ApprovedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Expenses.Add(budgetExpense);

            await _context.SaveChangesAsync();


            // Tạo công nợ cho các thành viên
            var members =
    _context.TripMembers
    .Where(x => x.TripGroupId == trip.Id)
    .ToList();

            int debtorCount = members.Count - 1;

            decimal splitAmount =
                debtorCount > 0
                ? model.Budget / debtorCount
                : 0;

            foreach (var member in members)
            {
                bool isPayer =
                    member.UserId == user.Id;

                _context.ExpenseSplits.Add(
                    new ExpenseSplit
                    {
                        ExpenseId = budgetExpense.Id,
                        UserId = member.UserId,

                        Amount = isPayer
                            ? 0
                            : splitAmount,

                        IsPaid = isPayer
                    });

                if (!isPayer)
                {
                    _context.ExpenseApprovals.Add(
                        new ExpenseApproval
                        {
                            ExpenseId = budgetExpense.Id,
                            UserId = member.UserId,
                            IsApproved = true,
                            ApprovedAt = DateTime.Now
                        });
                }
            }

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
            model.CompletedTrips =
                _context.TripGroups
                .Where(x =>
                    x.Status == "Completed"
                &&
                (
                    x.LeaderId == userId
                    ||
                    _context.TripMembers.Any(m =>
                        m.TripGroupId == x.Id &&
                        m.UserId == userId)
                ))
            .ToList();

            model.CreatedTrips =
                _context.TripGroups
                .Where(x =>
                    x.LeaderId == userId
                    && x.Status != "Completed")
                .ToList();

            model.JoinedTrips =
 (
     from tm in _context.TripMembers
     join t in _context.TripGroups
         on tm.TripGroupId equals t.Id
     where tm.UserId == userId
        && t.LeaderId != userId
        && t.Status != "Completed"
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



            ViewBag.UnpaidMembers =
(
    from split in _context.ExpenseSplits

    join expense in _context.Expenses
        on split.ExpenseId equals expense.Id

    join member in _context.Users
        on split.UserId equals member.Id

    where expense.TripGroupId == id
    && expense.IsApproved
    && split.Amount > 0
    && !split.IsPaid
    && split.UserId != expense.PaidByUserId

    select new
    {
        Email = member.Email,
        Amount = split.Amount
    }
).ToList();

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

            

            var user =
                await _userManager.GetUserAsync(User);

            var trip =
                _context.TripGroups
                .FirstOrDefault(x => x.Id == tripId);

            if (trip == null)
                return NotFound();

            if (trip.LeaderId != user!.Id)
                return Unauthorized();

            var request =
                _context.JoinRequests
                .FirstOrDefault(x =>
                    x.TripGroupId == tripId &&
                    x.UserId == userId);

            if (request == null)
                return NotFound();

            request.Status = "Approved";

            bool exists =
    _context.TripMembers.Any(x =>
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
                await _context.SaveChangesAsync();


                var budgetExpense =
                    _context.Expenses
                    .FirstOrDefault(x =>
                        x.TripGroupId == tripId &&
                        x.Title == "Ngân sách chuyến đi");

                if (budgetExpense != null)
                {
                    budgetExpense.IsApproved = true;
                    budgetExpense.ApprovedAt = DateTime.Now;

                    var memberIds =
                        _context.TripMembers
                        .Where(x => x.TripGroupId == tripId)
                        .Select(x => x.UserId)
                        .ToList();

                    int debtorCount =
                        memberIds.Count - 1;

                    decimal splitAmount =
                        debtorCount > 0
                        ? budgetExpense.Amount / debtorCount
                        : 0;

                    var oldSplits =
                        _context.ExpenseSplits
                        .Where(x => x.ExpenseId == budgetExpense.Id);

                    _context.ExpenseSplits.RemoveRange(oldSplits);

                    var oldApprovals =
                        _context.ExpenseApprovals
                        .Where(x => x.ExpenseId == budgetExpense.Id);

                    _context.ExpenseApprovals.RemoveRange(oldApprovals);

                    foreach (var memberId in memberIds)
                    {
                        bool isLeader =
                            memberId == budgetExpense.PaidByUserId;

                        _context.ExpenseSplits.Add(
                            new ExpenseSplit
                            {
                                ExpenseId = budgetExpense.Id,
                                UserId = memberId,
                                Amount = isLeader
                                    ? 0
                                    : splitAmount,
                                IsPaid = isLeader
                            });

                        if (!isLeader)
                        {
                            _context.ExpenseApprovals.Add(
                                new ExpenseApproval
                                {
                                    ExpenseId = budgetExpense.Id,
                                    UserId = memberId,
                                    IsApproved = true,
                                    ApprovedAt = DateTime.Now
                                });
                        }
                    }
                }
            }

            _context.Notifications.Add(
                new Notification
                {
                    UserId = userId,
                    Title = "Yêu cầu được chấp nhận",
                    Content = "Bạn đã được chấp nhận vào nhóm.",
                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = tripId });
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

            [HttpPost]
            public IActionResult CompleteTrip(int tripId)
            {
                var trip =
                    _context.TripGroups
                    .FirstOrDefault(x => x.Id == tripId);

                if (trip == null)
                    return NotFound();

            var unpaidMembers =
(
from split in _context.ExpenseSplits
join expense in _context.Expenses
    on split.ExpenseId equals expense.Id
join user in _context.Users
    on split.UserId equals user.Id

where expense.TripGroupId == tripId
    && expense.IsApproved
    && !split.IsPaid
    && split.Amount > 0
    && split.UserId != expense.PaidByUserId

select user.Email
)
.Distinct()
.ToList();

            if (unpaidMembers.Any())
                {
                    TempData["Error"] =
                        "Không thể hoàn thành chuyến đi. Thành viên chưa thanh toán: "
                        + string.Join(", ", unpaidMembers);

                    return RedirectToAction(
                        "Details",
                        new { id = tripId });
                }

                trip.Status = "Completed";

                _context.SaveChanges();

                TempData["Success"] =
                    "Chuyến đi đã được hoàn thành.";

                return RedirectToAction(nameof(MyTrips));
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
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    success = false
                });
            }

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

            return Json(new
            {
                success = true,
                userName = user.Email,
                message = message,
                time = DateTime.Now
                    .ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyService(
    int tripId,
    int serviceId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var service =
                _context.Services
                .FirstOrDefault(x => x.Id == serviceId);

            if (service == null)
                return RedirectToAction(
                    "Details",
                    new { id = tripId });

            

            var tripService = new TripService
            {
                TripGroupId = tripId,
                ServiceId = serviceId,
                AppliedByUserId = user!.Id,
                AppliedAt = DateTime.Now
            };

            _context.TripServices.Add(tripService);

            var booking = new Booking
            {
                TripGroupId = tripId,
                ServiceId = serviceId,
                UserId = user.Id,
                BookingDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);

            var expense = new Expense
            {
                TripGroupId = tripId,
                PaidByUserId = user.Id,
                Title = service.Name,
                Amount = service.Price,
                IsApproved = true,
                ApprovedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Expenses.Add(expense);

            await _context.SaveChangesAsync();

            var members =
    _context.TripMembers
    .Where(x => x.TripGroupId == tripId)
    .ToList();

            int debtorCount =
    members.Count - 1;

            decimal splitAmount =
                debtorCount > 0
                ? service.Price / debtorCount
                : 0;

            foreach (var member in members)
            {
                _context.ExpenseSplits.Add(
                    new ExpenseSplit
                    {
                        ExpenseId = expense.Id,

                        UserId = member.UserId,

                        Amount =
                            member.UserId == user.Id
                            ? 0
                            : splitAmount,

                        IsPaid =
                            member.UserId == user.Id
                    });
            }

            await _context.SaveChangesAsync();


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

            

            return RedirectToAction(
                "Details",
                new
                {
                    id = tripService.TripGroupId
                });




        }

        

        
        

        


    }
}