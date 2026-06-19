namespace HTQL_DU_LICH.ViewModels
{
    public class ExclusionRequestViewModel
    {
        public int RequestId { get; set; }

        public string UserName { get; set; } = "";

        public string ExpenseTitle { get; set; } = "";

        public string Reason { get; set; } = "";

        public string Status { get; set; } = "";

        

        // thêm

        public string TripName { get; set; } = "";

        public string ExpenseOwner { get; set; } = "";

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool CanApprove { get; set; }

    }
}