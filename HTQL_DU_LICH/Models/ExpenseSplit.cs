namespace HTQL_DU_LICH.Models
{
    public class ExpenseSplit
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public string UserId { get; set; } = "";

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }

        public bool TransferConfirmed { get; set; }

        public DateTime? PaidAt { get; set; }

        public bool ReceivedConfirmed { get; set; }

        public DateTime? ReceivedAt { get; set; }
    }
}