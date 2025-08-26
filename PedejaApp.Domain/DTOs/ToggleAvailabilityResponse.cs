namespace PedejaApp.Domain.DTOs
{
    // Story 2.3: Quick Operations DTOs
    public class ToggleAvailabilityResponse
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime Timestamp { get; set; }
    }
}