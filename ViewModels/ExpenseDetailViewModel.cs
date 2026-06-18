namespace HTQL_DU_LICH.ViewModels
{
    public class ExpenseDetailViewModel
    {
        public string ExpenseTitle { get; set; } = "";

        public string PaidBy { get; set; } = "";

        public decimal TotalAmount { get; set; }

        public decimal YourShare { get; set; }
    }
}