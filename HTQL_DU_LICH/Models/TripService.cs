namespace HTQL_DU_LICH.Models
{
    public class TripService
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public int ServiceId { get; set; }

        public string AppliedByUserId { get; set; } = "";

        public DateTime AppliedAt { get; set; }
            = DateTime.Now;
    }
}