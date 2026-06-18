namespace HTQL_DU_LICH.ViewModels
{
    public class MyBookingViewModel
    {
        public int Id { get; set; }

        public string ServiceName { get; set; } = "";

        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = "";

        public string TripName { get; set; } = "";

        public bool IsReviewed { get; set; }
    }
}