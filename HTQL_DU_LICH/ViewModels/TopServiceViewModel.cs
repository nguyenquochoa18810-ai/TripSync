namespace HTQL_DU_LICH.ViewModels
{
    public class TopServiceViewModel
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = "";

        public decimal Price { get; set; }

        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }
    }
}