namespace HTQL_DU_LICH.ViewModels
{
    public class ExpenseSummaryViewModel
    {
        public decimal TotalExpense { get; set; }

        public decimal YouOwe { get; set; }

        public decimal YouAreOwed { get; set; }

        public List<ExpenseDetailViewModel> Details { get; set; }
    = new List<ExpenseDetailViewModel>();

        public List<UserDebtDetailViewModel> DebtDetails
        { get; set; } = new();
    }
}