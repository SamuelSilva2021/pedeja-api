namespace PedejaApp.Domain.DTOs
{
    public class BulkUpdateResponse
    {
        public int SuccessCount { get; set; }
        public List<ToggleAvailabilityResponse> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}