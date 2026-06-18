using System.ComponentModel.DataAnnotations;

namespace HTQL_DU_LICH.ViewModels
{
    public class CreateVendorViewModel
    {
        [Required]
        public string CompanyName { get; set; } = "";

        public string Description { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Email { get; set; } = "";

        public string Address { get; set; } = "";
    }
}