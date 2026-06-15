

namespace HTQL_DU_LICH.Models
{
    public class Service
    {
        public int Id { get; set; }

        public int VendorId { get; set; }

        public string Name { get; set; } = "";

        public String ServiceType { get; set; } = "";

        public string Description { get; set; } = "";

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}