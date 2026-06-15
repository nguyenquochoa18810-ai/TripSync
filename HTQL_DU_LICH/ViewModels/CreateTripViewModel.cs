namespace HTQL_DU_LICH.ViewModels
{
    public class CreateTripViewModel
    {
        public string Title { get; set; } = "";

    public string Destination { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string? Description { get; set; }

        public List<int> SelectedInterests { get; set; }
            = new List<int>();
    }

}