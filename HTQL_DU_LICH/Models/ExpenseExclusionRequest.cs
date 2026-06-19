namespace HTQL_DU_LICH.Models
{
    public class ExpenseExclusionRequest
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public string UserId { get; set; } = "";

        public string Reason { get; set; } = "";

        public string Status { get; set; }
            = "Pending";

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}