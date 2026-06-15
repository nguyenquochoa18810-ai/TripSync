namespace HTQL_DU_LICH.ViewModels
{
    public class MatchResultViewModel
    {
        public int TripId { get; set; }

        public string TripTitle { get; set; } = "";

        public int MatchPercent { get; set; }

        public List<string> Interests { get; set; }
            = new();

        public string LeaderName { get; set; } = "";

        public string Status { get; set; } = "";

        public int MemberCount { get; set; }

        public int CommonInterestCount { get; set; }

        public int TotalTripInterestCount { get; set; }


    }
}