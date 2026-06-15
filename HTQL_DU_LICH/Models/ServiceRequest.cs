namespace HTQL_DU_LICH.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public int ServiceId { get; set; }

        public string CreatedByUserId { get; set; } = "";

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}
