using System.ComponentModel.DataAnnotations;


namespace HTQL_DU_LICH.Models
{
    public class Service
    {
        public int Id { get; set; }

        public int VendorId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn loại dịch vụ")]
        public string ServiceType { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; } = "";

        [Range(0, double.MaxValue,
            ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ImageUrl { get; set; }
    }
}