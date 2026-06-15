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
                select new ExpenseListViewModel
                {

                    ApprovedCount =
                    _context.ExpenseApprovals
                    .Count(x =>
                        x.ExpenseId == e.Id &&
                        x.IsApproved),

                    TotalApprovals =
                    _context.ExpenseApprovals
                    .Count(x =>
                        x.ExpenseId == e.Id),

                    Id = e.Id,

                    TripName = t.Title,

                    Title = e.Title,

                    Amount = e.Amount,

                    PaidBy =
                        string.IsNullOrEmpty(u.FullName)
                        ? u.Email
                        : u.FullName,

                    IsApproved = e.IsApproved,

                    CanApprove =
                        _context.ExpenseApprovals.Any(x =>
                        x.ExpenseId == e.Id &&
                        x.UserId == user.Id &&
                        !x.IsApproved)
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

                IsApproved = false
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync(); // Dòng SaveChanges thứ nhất

            var members =
                _context.TripMembers
                .Where(x => x.TripGroupId == model.TripGroupId)
                .ToList();

            decimal splitAmount =
                model.Amount / members.Count;

            foreach (var member in members)
            {
                _context.ExpenseSplits.Add(
                    new ExpenseSplit
                    {
                        ExpenseId = expense.Id,
                        UserId = member.UserId,
                        Amount = splitAmount,
                        IsPaid = false
                    });
            }

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

            await _context.SaveChangesAsync();

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

            var totalExpense =
                _context.Expenses
                .Where(x => x.IsApproved)
                .Sum(x => x.Amount);

            var youOwe =
            (
                from split in _context.ExpenseSplits
                join expense in _context.Expenses
                    on split.ExpenseId equals expense.Id
                where split.UserId == user.Id
                    && expense.PaidByUserId != user.Id
                    && expense.IsApproved
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
                        .Count(x => x.ExpenseId == expense.Id),

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
                    && expense.IsApproved
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

                where split.UserId != expense.PaidByUserId
                    && split.UserId == user.Id
                    && expense.IsApproved

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

                    // Thông báo cho người tạo khoản chi
                    _context.Notifications.Add(
                        new Notification
                        {
                            UserId = expense.PaidByUserId,

                            Title = "Khoản chi đã được duyệt",

                            Content =
                                $"Khoản chi '{expense.Title}' đã được tất cả thành viên xác nhận.",

                            IsRead = false,

                            CreatedAt = DateTime.Now
                        });

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectExpense(int expenseId)
        {
            return Content("Reject OK - ExpenseId = " + expenseId);
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

        public IActionResult Transactions()
        {
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
                    && expense.IsApproved

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

            ).OrderByDescending(x => x.PaidAt)
             .ToList();

            return View(data);
        }

    }
}