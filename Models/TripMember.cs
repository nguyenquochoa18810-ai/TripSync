namespace HTQL_DU_LICH.Models
{
    public class TripMember
    {
        public int TripGroupId { get; set; }

        public string UserId { get; set; } = "";
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }

}