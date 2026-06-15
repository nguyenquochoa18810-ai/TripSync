namespace HTQL_DU_LICH.Models
{
    public class ServiceRequestVote
    {
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        public string UserId { get; set; } = "";

        public bool IsApproved { get; set; }
    }
}