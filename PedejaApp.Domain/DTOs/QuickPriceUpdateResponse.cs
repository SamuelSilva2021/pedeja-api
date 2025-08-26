namespace PedejaApp.Domain.DTOs
{
    public class QuickPriceUpdateResponse
    {
        public int Id { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime Timestamp { get; set; }
    }
}