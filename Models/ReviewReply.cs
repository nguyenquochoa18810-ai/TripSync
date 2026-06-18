namespace HTQL_DU_LICH.Models
{
    public class ReviewReply
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }

        public int VendorId { get; set; }

        public string ReplyContent { get; set; } = "";

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}