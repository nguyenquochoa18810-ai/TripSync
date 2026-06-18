namespace HTQL_DU_LICH.ViewModels
{
    public class TransactionHistoryViewModel
    {
        public string TripName { get; set; } = "";

        public string ExpenseTitle { get; set; } = "";

        public string Sender { get; set; } = "";

        public string Receiver { get; set; } = "";

        public decimal Amount { get; set; }

        public DateTime? PaidAt { get; set; }

        public bool IsCompleted { get; set; }
    }
}