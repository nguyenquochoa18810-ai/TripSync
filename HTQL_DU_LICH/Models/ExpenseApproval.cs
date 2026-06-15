namespace HTQL_DU_LICH.Models
{
    public class ExpenseApproval
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public string UserId { get; set; } = "";

        public bool IsApproved { get; set; }

        public DateTime? ApprovedAt { get; set; }

        
    }
}