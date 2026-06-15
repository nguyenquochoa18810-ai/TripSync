namespace HTQL_DU_LICH.ViewModels
{
    public class VendorDashboardViewModel
    {
        public int TotalServices { get; set; }

        public int TotalBookings { get; set; }

        public int PendingBookings { get; set; }

        public int ConfirmedBookings { get; set; }

        public int CompletedBookings { get; set; }

        public int CancelledBookings { get; set; }

        public int TotalReviews { get; set; }

        public double AverageRating { get; set; }
    }
}