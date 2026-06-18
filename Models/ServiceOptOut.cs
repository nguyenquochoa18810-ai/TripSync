namespace HTQL_DU_LICH.Models
{
    // Lưu lại thành viên KHÔNG sử dụng một dịch vụ trong chuyến đi
    // -> sẽ không bị chia tiền cho dịch vụ đó
    public class ServiceOptOut
    {
        public int Id { get; set; }

        public int TripGroupId { get; set; }

        public int ServiceId { get; set; }

        public string UserId { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
