namespace HTQL_DU_LICH.ViewModels
{
    public class RejectRequestViewModel
    {
        public int RequestId { get; set; }

        public string TripName { get; set; } = "";

        public string ExpenseTitle { get; set; } = "";

        public decimal Amount { get; set; }

        public string RequestedBy { get; set; } = "";
    }
}
