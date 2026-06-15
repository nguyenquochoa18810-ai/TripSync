namespace HTQL_DU_LICH.Models
{
    public class ServiceVote
    {
        public int Id { get; set; }

        public int TripServiceId { get; set; }

        public string UserId { get; set; } = "";

        public bool IsApproved { get; set; }
    }
}