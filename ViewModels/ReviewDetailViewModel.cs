namespace HTQL_DU_LICH.ViewModels
{
    public class ReviewDetailViewModel
    {
        public string UserName { get; set; } = "";

        public int Rating { get; set; }

        public string Comment { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}