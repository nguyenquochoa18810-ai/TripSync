namespace HTQL_DU_LICH.ViewModels
{
    public class DebtViewModel
    {
        public string TripName { get; set; } = "";
        public string DebtorName { get; set; } = "";

        public string CreditorName { get; set; } = "";

        public decimal Amount { get; set; }

        public string ExpenseTitle { get; set; }

        public bool IsPaid { get; set; }

        public int ExpenseSplitId { get; set; }
    }
}