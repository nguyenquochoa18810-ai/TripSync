using HTQL_DU_LICH.Data;
using HTQL_DU_LICH.Models;
using HTQL_DU_LICH.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HTQL_DU_LICH.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpenseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult TestRoute()
        {
            return Content("Expense Controller OK");
        }

        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            var tripIds =
                _context.TripMembers
                .Where(x => x.UserId == user.Id)
                .Select(x => x.TripGroupId)
                .ToList();

            var expenses =
            (
                from e in _context.Expenses
                join u in _context.Users
                    on e.PaidByUserId equals u.Id
                join t in _context.TripGroups
                    on e.TripGroupId equals t.Id
                where tripIds.Contains(e.TripGroupId)
                let request =
    _context.ExpenseExclusionRequests
    .Where(r =>
        r.ExpenseId == e.Id &&
        r.Status == "Pending")
    .OrderByDescending(r => r.Id)
    .FirstOrDefault()

                select new ExpenseListViewModel
                {
                    Id = e.Id,

                    TripName = t.Title,

                    Title = e.Title,

                    Amount = e.Amount,

                    PaidBy =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    IsApproved = e.IsApproved,

                    IsExpenseOwner =
                        e.PaidByUserId == user.Id,

                    PendingRequestCount =
                        _context.ExpenseExclusionRequests
                        .Count(x =>
                            x.ExpenseId == e.Id &&
                            x.Status == "Pending"),

                    RequestId =
                        request != null
                        ? request.Id
                        : null,

                    RequestStatus =
                        request != null
                        ? request.Status
                        : null,

                    RequestUserName =
                        request != null
                        ? _context.Users
                            .Where(x => x.Id == request.UserId)
                            .Select(x => x.Email)
                            .FirstOrDefault()
                        : null,

                    RequestReason =
                        request != null
                        ? request.Reason
                        : null,

                    ApprovedCount =
                        _context.ExpenseApprovals
                        .Count(x =>
                            x.ExpenseId == e.Id &&
                            x.IsApproved),

                    TotalApprovals =
                        _context.ExpenseApprovals
                        .Count(x =>
                            x.ExpenseId == e.Id),

                    CanApprove =
                        _context.ExpenseApprovals.Any(x =>
                            x.ExpenseId == e.Id &&
                            x.UserId == user.Id &&
                            !x.IsApproved),

                    HasPendingRequest =
                        _context.ExpenseExclusionRequests.Any(x =>
                            x.ExpenseId == e.Id &&
                            x.UserId == user.Id &&
                            x.Status == "Pending")
                }
            ).ToList();

            return View(expenses);
        }

        [HttpGet]
        public IActionResult Create(int tripId)
        {
            return View(new CreateExpenseViewModel
            {
                TripGroupId = tripId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var expense = new Expense
            {
                TripGroupId = model.TripGroupId,
                PaidByUserId = user.Id,
                Title = model.Title,
                Amount = model.Amount,
                CreatedAt = DateTime.Now,

                IsApproved = false,
                ApprovedAt = null
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync(); // Dòng SaveChanges thứ nhất

            var members =
                _context.TripMembers
                .Where(x => x.TripGroupId == model.TripGroupId)
                .ToList();

            Console.WriteLine(
                $"TripId = {model.TripGroupId}");

            Console.WriteLine(
                $"Members Count = {members.Count}");

            foreach (var m in members)
            {
                Console.WriteLine(
                    $"Member = {m.UserId}");
            }

            if (members.Count == 0)
            {
                throw new Exception(
                    $"Trip {model.TripGroupId} không có thành viên");
            }

            int memberCount = members.Count;

            decimal splitAmount =
                memberCount > 0
                ? model.Amount / memberCount
                : 0;

            foreach (var member in members)
            {
                bool isPayer =
                    member.UserId == user.Id;

                _context.ExpenseSplits.Add(
                    new ExpenseSplit
                    {
                        ExpenseId = expense.Id,
                        UserId = member.UserId,

                        Amount = splitAmount,


                        IsPaid = isPayer
                    });

                Console.WriteLine(
    $"ExpenseId = {expense.Id}");

                Console.WriteLine(
                    $"Split Count Before Save = " +
                    _context.ExpenseSplits.Local.Count);
            }

            Console.WriteLine(
    $"ExpenseId = {expense.Id}");

            Console.WriteLine(
                $"Members Count = {members.Count}");

            Console.WriteLine(
                $"ExpenseSplits Local = " +
                _context.ExpenseSplits.Local.Count);
            foreach (var member in members)
            {
                if (member.UserId != user.Id)
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


            try
            {
                await _context.SaveChangesAsync();

                Console.WriteLine(
                    "ExpenseSplit + Approval SAVED");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SAVE ERROR");

                Console.WriteLine(
                    ex.ToString());

                throw;
            }

            var splitCount =
                _context.ExpenseSplits
                .Count(x => x.ExpenseId == expense.Id);

            Console.WriteLine(
                $"Split In Database = {splitCount}");



            // ==========================================
            // THÊM ĐOẠN THÔNG BÁO CHO CÁC THÀNH VIÊN KHÁC VÀO ĐÂY
            // ==========================================
            foreach (var member in members)
            {
                if (member.UserId != user.Id)
                {
                    _context.Notifications.Add(
                        new Notification
                        {
                            UserId = member.UserId,
                            Title = "Chi phí mới",
                            Content = $"{user.FullName} vừa thêm khoản chi {model.Title}",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        });
                }
            }
            await _context.SaveChangesAsync();

            // Thông báo cho chính người tạo chi phí
            _context.Notifications.Add(
                new Notification
                {
                    UserId = user.Id,
                    Title = "Khoản chi cần xác nhận",
                    Content =
                          $"{user.FullName} vừa thêm khoản chi '{model.Title}'",

                    IsRead = false,

                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync(); // Dòng SaveChanges cuối cùng trước khi chuyển hướng

            return RedirectToAction(nameof(Index));
        }

        //ACTION SUMMARY
        public async Task<IActionResult> Summary()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.UserId = user.Id;
            ViewBag.Email = user.Email;

            var tripIds =
                _context.TripMembers
                .Where(x => x.UserId == user.Id)
                .Select(x => x.TripGroupId)
                .ToList();

            var totalExpense =
                _context.Expenses
                .Where(x =>
                    tripIds.Contains(x.TripGroupId)
                    && x.IsApproved)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            var youOwe =
            (
            from split in _context.ExpenseSplits
            join expense in _context.Expenses
                on split.ExpenseId equals expense.Id

            where split.UserId == user.Id
    && expense.PaidByUserId != user.Id
    && expense.IsApproved
    && split.Amount > 0
    && !split.IsPaid
    && tripIds.Contains(expense.TripGroupId)

            select split.Amount
            ).Sum();

            var debtDetails =
            (
                from split in _context.ExpenseSplits

                join expense in _context.Expenses
                    on split.ExpenseId equals expense.Id

                join payer in _context.Users
                    on expense.PaidByUserId equals payer.Id

                where split.UserId == user.Id
&& expense.PaidByUserId != user.Id
&& expense.IsApproved
&& split.Amount > 0
&& tripIds.Contains(expense.TripGroupId)

                select new UserDebtDetailViewModel
                {
                    ExpenseTitle = expense.Title,

                    PaidBy =
                        string.IsNullOrEmpty(payer.FullName)
                        ? payer.Email
                        : payer.FullName,

                    ExpenseAmount = expense.Amount,

                    MemberCount =
_context.ExpenseSplits
.Count(x =>
    x.ExpenseId == expense.Id
    && x.Amount > 0),

                    YourShare = split.Amount
                }
            ).ToList();

            var youAreOwed =
(
    from split in _context.ExpenseSplits

    join expense in _context.Expenses
        on split.ExpenseId equals expense.Id

    where expense.PaidByUserId == user.Id
        && split.UserId != user.Id
        && split.UserId != expense.PaidByUserId
        && expense.IsApproved
        && !split.IsPaid
        && split.Amount > 0
        && tripIds.Contains(expense.TripGroupId)

    select split.Amount
).Sum();

            var model = new ExpenseSummaryViewModel
            {
                TotalExpense = totalExpense,
                YouOwe = youOwe,
                YouAreOwed = youAreOwed,

                
                DebtDetails = debtDetails
            }; 

            return View(model);
        }

        public IActionResult Debts()
        {
            var user =
                _userManager.GetUserAsync(User).Result;

            var debts =
            (
                from split in _context.ExpenseSplits

                join expense in _context.Expenses
                    on split.ExpenseId equals expense.Id

                join trip in _context.TripGroups
                    on expense.TripGroupId equals trip.Id

                join debtor in _context.Users
                    on split.UserId equals debtor.Id

                join creditor in _context.Users
                    on expense.PaidByUserId equals creditor.Id

                where split.Amount > 0
&& !split.IsPaid
&& expense.IsApproved
&& split.UserId != expense.PaidByUserId

                select new DebtViewModel
                {
                    ExpenseSplitId = split.Id,

                    TripName = trip.Title,

                    ExpenseTitle = expense.Title,

                    DebtorName =
                        string.IsNullOrEmpty(debtor.FullName)
                        ? debtor.Email
                        : debtor.FullName,

                    CreditorName =
                        string.IsNullOrEmpty(creditor.FullName)
                        ? creditor.Email
                        : creditor.FullName,

                    Amount = split.Amount,

                    IsPaid = split.IsPaid
                }

            ).ToList();

            Console.WriteLine(
                $"Debt Count = {debts.Count}");

            foreach (var item in debts)
            {
                Console.WriteLine(
                    $"{item.ExpenseTitle} - {item.Amount}");
            }

            return View(debts);
        }

        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var split =
                _context.ExpenseSplits
                .FirstOrDefault(x => x.Id == id);

            if (split == null)
                return NotFound();

            split.IsPaid = true;

            split.TransferConfirmed = true;

            split.PaidAt = DateTime.Now;

            var expense =
                _context.Expenses
                .FirstOrDefault(x => x.Id == split.ExpenseId);

            if (expense != null)
            {
                var payer =
                    await _userManager.GetUserAsync(User);

                _context.Notifications.Add(

                    new Notification
                    {
                        UserId = expense.PaidByUserId,

                        Title = "Thanh toán hoàn tất",

                        Content =
                            $"{payer?.Email} đã xác nhận thanh toán khoản '{expense.Title}'",

                        IsRead = false,

                        CreatedAt = DateTime.Now
                    });

                var currentUser =
                    await _userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    _context.Notifications.Add(
                        new Notification
                        {
                            UserId = currentUser.Id,

                            Title = "Đã thanh toán",

                            Content =
                                $"Bạn đã thanh toán khoản '{expense.Title}' thành công",

                            IsRead = false,

                            CreatedAt = DateTime.Now
                        });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Debts));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveExpense(int expenseId)
        {
            var user =
                await _userManager.GetUserAsync(User);

            var approval =
                _context.ExpenseApprovals
                .FirstOrDefault(x =>
                    x.ExpenseId == expenseId &&
                    x.UserId == user.Id);

            if (approval == null)
                return NotFound();
            
            approval.IsApproved = true;

            approval.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var allApproved =
                _context.ExpenseApprovals
                .Where(x => x.ExpenseId == expenseId)
                .All(x => x.IsApproved);

            if (allApproved)
            {
                var expense =
                    _context.Expenses
                    .FirstOrDefault(x => x.Id == expenseId);

                if (expense != null)
                {
                    expense.IsApproved = true;

                    var memberIds =
                        _context.TripMembers
                        .Where(x => x.TripGroupId == expense.TripGroupId)
                        .Select(x => x.UserId)
                        .ToList();

                    foreach (var memberId in memberIds)
                    {
                        _context.Notifications.Add(
                            new Notification
                            {
                                UserId = memberId,

                                Title = "Khoản chi đã được duyệt",

                                Content =
                                    $"Khoản chi '{expense.Title}' đã được tất cả thành viên xác nhận.",

                                IsRead = false,

                                CreatedAt = DateTime.Now
                            });
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

     
        [HttpGet]
        public IActionResult ExpenseDetails(int id)
        {
            var expense =
                _context.Expenses
                .FirstOrDefault(x => x.Id == id);

            if (expense == null)
                return NotFound();

            var payer =
                _context.Users
                .FirstOrDefault(x => x.Id == expense.PaidByUserId);

            var approvals =
            (
                from a in _context.ExpenseApprovals
                join u in _context.Users
                    on a.UserId equals u.Id
                where a.ExpenseId == id
                select new ExpenseApprovalStatusViewModel
                {
                    UserName =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    IsApproved = a.IsApproved
                }
            ).ToList();

            var model = new ExpenseDetailsViewModel
            {
                ExpenseId = expense.Id,

                ExpenseTitle = expense.Title,

                Amount = expense.Amount,

                PaidBy =
                    string.IsNullOrEmpty(payer.FullName)
                    ? payer.Email
                    : payer.FullName,

                IsApproved = expense.IsApproved,

                ApprovedCount =
                    approvals.Count(x => x.IsApproved),

                TotalApprovals =
                    approvals.Count,

                Approvals = approvals
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult RejectExpense(int expenseId)
        {
            ViewBag.ExpenseId = expenseId;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RejectExpense(
    int expenseId,
    string reason)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Index));

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do.";

                ViewBag.ExpenseId = expenseId;

                return View();
            }

            var expense =
                _context.Expenses
                .FirstOrDefault(x => x.Id == expenseId);

            if (expense == null)
                return NotFound();

            bool exists =
                _context.ExpenseExclusionRequests.Any(x =>
                    x.ExpenseId == expenseId &&
                    x.UserId == user.Id &&
                    x.Status == "Pending");

            if (exists)
            {
                TempData["Error"] =
                    "Bạn đã gửi yêu cầu rồi.";

                return RedirectToAction(nameof(Index));
            }
            var request =
                new ExpenseExclusionRequest
                {
                    ExpenseId = expenseId,
                    UserId = user.Id,
                    Reason = reason,
                    Status = "Pending"
                };

            _context.ExpenseExclusionRequests
                .Add(request);

            var trip =
                _context.TripGroups
                .FirstOrDefault(x =>
                    x.Id == expense.TripGroupId);

            if (trip != null)
            {
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = expense.PaidByUserId,
                        Title = "Yêu cầu miễn chia tiền",

                        Content =
                            $"{user.Email} muốn miễn khoản chi '{expense.Title}'",

                        CreatedAt = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult>
        ApproveExclusionRequest(int requestId)
        {
            Console.WriteLine(
                $"ApproveExclusionRequest: {requestId}");
            var request =
                _context.ExpenseExclusionRequests
                .FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();
            var currentUser =
                await _userManager.GetUserAsync(User);

            var expense =
                _context.Expenses
                .FirstOrDefault(x =>
                    x.Id == request.ExpenseId);

            if (expense == null)
                return NotFound();

            if (expense.PaidByUserId != currentUser.Id)
            {
                return Unauthorized();
            }

            request.Status = "Approved";

            Console.WriteLine(
                $"Status changed to Approved");
            var allSplits =
    _context.ExpenseSplits
    .Where(x => x.ExpenseId == request.ExpenseId)
    .ToList();

            var excludedSplit =
    allSplits.FirstOrDefault(x =>
        x.UserId == request.UserId);

            // Người được miễn
            if (excludedSplit != null)
            {
                excludedSplit.Amount = 0;
                excludedSplit.IsPaid = true;
            }

            // Danh sách còn tham gia chia tiền
            var activeMembers =
                allSplits
                .Where(x => x.UserId != request.UserId)
                .ToList();

            int activeCount = activeMembers.Count;

            decimal newShare =
                activeCount > 0
                ? expense.Amount / activeCount
                : 0;

            // Chia lại tiền cho tất cả thành viên còn lại
            foreach (var item in activeMembers)
            {
                item.Amount = newShare;

                if (item.UserId == expense.PaidByUserId)
                {
                    item.IsPaid = true;
                }
                else
                {
                    item.IsPaid = false;
                }
            }



            var payerSplit =
                allSplits.FirstOrDefault(x =>
                    x.UserId == expense.PaidByUserId);

            if (payerSplit != null)
            {
                payerSplit.Amount = 0;
                payerSplit.IsPaid = true;
            }

            var split =
                _context.ExpenseSplits
                .FirstOrDefault(x =>
                    x.ExpenseId == request.ExpenseId
                    && x.UserId == request.UserId);

            if (split != null)
            {
                split.Amount = 0;
                split.IsPaid = true;
            }

            var approval =
                _context.ExpenseApprovals
                .FirstOrDefault(x =>
                    x.ExpenseId == request.ExpenseId
                    && x.UserId == request.UserId);

            if (approval != null)
            {
                approval.IsApproved = true;
                approval.ApprovedAt = DateTime.Now;
            }

            // ===== THÊM ĐOẠN NÀY =====

            

            expense.IsApproved = true;
            expense.ApprovedAt = DateTime.Now;

            var memberIds =
                _context.TripMembers
                .Where(x => x.TripGroupId == expense.TripGroupId)
                .Select(x => x.UserId)
                .ToList();

            foreach (var memberId in memberIds)
            {
                _context.Notifications.Add(
                    new Notification
                    {
                        UserId = memberId,
                        Title = "Khoản chi đã được duyệt",
                        Content =
                            $"Khoản chi '{expense.Title}' đã hoàn tất xác nhận.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
            }

            // =========================

            _context.Notifications.Add(
                new Notification
                {
                    UserId = request.UserId,

                    Title = "Yêu cầu được chấp thuận",

                    Content =
                        "Trưởng nhóm đã chấp thuận miễn chia tiền.",

                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ExclusionRequests));
        }

        [HttpPost]
        public async Task<IActionResult>
            RejectExclusionRequest(int requestId)
            {
            var request =
                _context.ExpenseExclusionRequests
                .FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();

            var currentUser =
                await _userManager.GetUserAsync(User);

            var expense =
                _context.Expenses
                .FirstOrDefault(x =>
                    x.Id == request.ExpenseId);

            if (expense == null)
                return NotFound();

            if (expense.PaidByUserId != currentUser.Id)
            {
                return Unauthorized();
            }

            request.Status = "Rejected";

            _context.Notifications.Add(
                new Notification
                {
                    UserId = request.UserId,

                    Title = "Yêu cầu bị từ chối",

                    Content =
                        "Trưởng nhóm đã từ chối yêu cầu miễn chia tiền.",

                    CreatedAt = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ExclusionRequests));
        }

        public async Task<IActionResult> Transactions()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var tripIds =
                _context.TripMembers
                .Where(x => x.UserId == user.Id)
                .Select(x => x.TripGroupId)
                .ToList();

            var data =
            (
                from split in _context.ExpenseSplits

                join expense in _context.Expenses
                    on split.ExpenseId equals expense.Id

                join trip in _context.TripGroups
                    on expense.TripGroupId equals trip.Id

                join debtor in _context.Users
                    on split.UserId equals debtor.Id

                join creditor in _context.Users
                    on expense.PaidByUserId equals creditor.Id

                where split.IsPaid
    && split.Amount > 0
    && split.UserId != expense.PaidByUserId
    && expense.IsApproved
    && tripIds.Contains(expense.TripGroupId)

                select new TransactionHistoryViewModel
                {
                    TripName = trip.Title,

                    ExpenseTitle = expense.Title,

                    Sender =
                        string.IsNullOrEmpty(debtor.FullName)
                        ? debtor.Email
                        : debtor.FullName,

                    Receiver =
                        string.IsNullOrEmpty(creditor.FullName)
                        ? creditor.Email
                        : creditor.FullName,

                    Amount = split.Amount,

                    PaidAt = split.PaidAt,

                    IsCompleted = split.TransferConfirmed
                }

            )
            .OrderByDescending(x => x.PaidAt)
            .ToList();

            return View(data);
        }



        [HttpGet]
        public async Task<IActionResult> ExclusionRequests()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Index));

            var requests =
            (
                from r in _context.ExpenseExclusionRequests

                join e in _context.Expenses
                    on r.ExpenseId equals e.Id

                join u in _context.Users
                    on r.UserId equals u.Id

                join t in _context.TripGroups
                    on e.TripGroupId equals t.Id

                where
                    _context.TripMembers.Any(m =>
                        m.TripGroupId == t.Id &&
                        m.UserId == user.Id)

                select new ExclusionRequestViewModel
                {
                    RequestId = r.Id,

                    UserName =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    ExpenseTitle = e.Title,

                    Reason = r.Reason,

                    Status = r.Status,


                    CanApprove = (e.PaidByUserId == user.Id),

                    TripName = t.Title,

                    Amount = e.Amount,

                    ExpenseOwner =
                        _context.Users
                        .Where(x => x.Id == e.PaidByUserId)
                        .Select(x => x.Email)
                        .FirstOrDefault(),

                    CreatedAt = r.CreatedAt
                }

            ).ToList();

            return View(requests);
        }


    }
}