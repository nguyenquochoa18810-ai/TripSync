using System.ComponentModel.DataAnnotations;

namespace HTQL_DU_LICH.ViewModels
{
    public class VendorRegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên doanh nghiệp")]
        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = "";
    }
}