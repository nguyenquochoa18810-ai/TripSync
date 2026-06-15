namespace HTQL_DU_LICH.Models
{
    public class JoinRequest
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public string UserId { get; set; } = "";

        public DateTime RequestDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}