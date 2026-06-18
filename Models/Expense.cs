namespace HTQL_DU_LICH.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public string PaidByUserId { get; set; } = "";

        public string Title { get; set; } = "";

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; }

        public DateTime? ApprovedAt { get; set; }

       
    }
}