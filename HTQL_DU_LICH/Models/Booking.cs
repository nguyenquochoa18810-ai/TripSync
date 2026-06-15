namespace HTQL_DU_LICH.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public int TripGroupId { get; set; }

        public int ServiceId { get; set; }

        public DateTime BookingDate { get; set; }
            = DateTime.Now;

        public string Status { get; set; }
            = "Pending";
    }
}