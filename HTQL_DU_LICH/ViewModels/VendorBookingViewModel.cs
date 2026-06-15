namespace HTQL_DU_LICH.ViewModels
{
    public class VendorBookingViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = "";

        public string ServiceName { get; set; } = "";

        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = "";
    }
}