namespace MixFlix.Web.Models
{
    public class ContentAvailabilityResponse
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceLogo { get; set; }
        public int? Ranking { get; set; }
    }
}
