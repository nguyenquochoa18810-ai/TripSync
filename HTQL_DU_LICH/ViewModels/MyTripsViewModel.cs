using HTQL_DU_LICH.Models;

namespace HTQL_DU_LICH.ViewModels
{
    public class MyTripsViewModel
    {
        public List<TripGroup> CreatedTrips { get; set; }
            = new();

        public List<TripGroup> JoinedTrips { get; set; }
            = new();

        public List<TripGroup> PendingTrips { get; set; }
            = new();

        public List<TripGroup> CompletedTrips
        { get; set; }
            = new();
    }
}