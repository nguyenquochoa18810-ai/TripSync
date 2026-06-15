using System.Collections.Generic;

namespace HTQL_DU_LICH.ViewModels
{
    public class ExpenseDetailsViewModel
    {
        public int ExpenseId { get; set; }

        public string ExpenseTitle { get; set; } = "";

        public decimal Amount { get; set; }

        public string PaidBy { get; set; } = "";

        public List<ExpenseApprovalStatusViewModel> Approvals
        { get; set; } = new();

        public bool IsApproved { get; set; }

        public int ApprovedCount { get; set; }

        public int TotalApprovals { get; set; }
    }
}