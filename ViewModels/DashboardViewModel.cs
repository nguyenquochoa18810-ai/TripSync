namespace HTQL_DU_LICH.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTrips { get; set; }

        public int TotalMembers { get; set; }

        public int TotalNotifications { get; set; }

        public int PendingRequests { get; set; }

        public decimal TotalExpense { get; set; }

        public string? LatestTripName { get; set; }

        public string? LatestBookingService { get; set; }

        public string? LatestBookingStatus { get; set; }

        public decimal MyDebt { get; set; }
    }
}