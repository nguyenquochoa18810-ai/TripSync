namespace HTQL_DU_LICH.ViewModels
{
    public class ExpenseListViewModel
    {

        public int Id { get; set; }

        public string Title { get; set; } = "";

        public decimal Amount { get; set; }

        public string PaidBy { get; set; } = "";

        public string TripName { get; set; } = "";

        public bool IsApproved { get; set; }

        public bool CanApprove { get; set; }

        public int ApprovedCount { get; set; }

        public int TotalApprovals { get; set; }

        public bool HasPendingRequest { get; set; }

        public bool IsExpenseOwner { get; set; }

        public int? RequestId { get; set; }

        public string? RequestStatus { get; set; }

        public string? RequestUserName { get; set; }

        public string? RequestReason { get; set; }



        public int PendingRequestCount { get; set; }
    }
}