namespace HTQL_DU_LICH.ViewModels
{
    public class UserDebtDetailViewModel
    {
        public string ExpenseTitle { get; set; } = "";

        public string PaidBy { get; set; } = "";

        public decimal ExpenseAmount { get; set; }

        public int MemberCount { get; set; }

        public decimal YourShare { get; set; }
    }
}