namespace HTQL_DU_LICH.ViewModels
{
    public class ServiceReviewSummaryViewModel
    {
        public string ServiceName { get; set; } = "";

        public double AverageRating { get; set; }

        public int FiveStar { get; set; }

        public int FourStar { get; set; }

        public int ThreeStar { get; set; }

        public int TwoStar { get; set; }

        public int OneStar { get; set; }

        public int TotalReviews { get; set; }
    }
}