namespace HTQL_DU_LICH.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public string UserId { get; set; } = "";

        public int VendorId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}