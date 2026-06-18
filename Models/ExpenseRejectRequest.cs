namespace HTQL_DU_LICH.Models
{
    // Khi thành viên bấm "Từ chối" 1 khoản chi
    // -> tạo yêu cầu để TRƯỞNG NHÓM xác nhận
    public class ExpenseRejectRequest
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public string RequestedByUserId { get; set; } = "";

        // Pending / Approved / Rejected
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
