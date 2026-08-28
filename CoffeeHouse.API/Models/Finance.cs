namespace CoffeeHouse.API.Models
{
    public class Finance
    {
        public long FinanceId { get; set; }

        public string Type { get; set; } = "";

        public string Description { get; set; } = "";

        public decimal Amount { get; set; }

        public DateTime FinanceDate { get; set; }

        public string Status { get; set; } = "";

        public string Note { get; set; } = "";
    }
}