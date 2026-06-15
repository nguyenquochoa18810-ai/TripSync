namespace HTQL_DU_LICH.Models
{
    public class UserInterest
    {
        public string UserId { get; set; } = "";

        public int InterestId { get; set; }

        public ApplicationUser? User { get; set; }

        public Interest? Interest { get; set; }
    }
}