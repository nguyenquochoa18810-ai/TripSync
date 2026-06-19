namespace HTQL_DU_LICH.Models
{
    public class TripGroup
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Destination { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string LeaderId { get; set; } = "";
        public string? Description { get; set; }
        public string? CoverImage { get; set; }

        public string Status { get; set; } = "Active";


    }

}