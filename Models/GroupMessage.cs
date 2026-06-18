namespace HTQL_DU_LICH.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public string UserId { get; set; } = "";

        public string Message { get; set; } = "";

        public DateTime SentAt { get; set; }
            = DateTime.Now;

        
    }
}