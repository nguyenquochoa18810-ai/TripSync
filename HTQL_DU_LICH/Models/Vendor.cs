namespace HTQL_DU_LICH.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public string CompanyName { get; set; } = "";

        public string Description { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Email { get; set; } = "";

        public string Address { get; set; } = "";

        public bool IsApproved { get; set; }
    }
}