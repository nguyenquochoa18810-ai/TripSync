using System.ComponentModel.DataAnnotations;

namespace HTQL_DU_LICH.ViewModels
{
    public class VendorRegisterViewModel
    {
        [Required]
        public string CompanyName { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Phone { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";
    }
}