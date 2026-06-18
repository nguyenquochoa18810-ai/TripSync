namespace HTQL_DU_LICH.ViewModels
{
    public class CreateExpenseViewModel
    {
        public int TripGroupId { get; set; }

        public string Title { get; set; } = "";

        public decimal Amount { get; set; }
    }
}