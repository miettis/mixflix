using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class ContentResponse
    {
        public Guid Id { get; set; }
        public ContentType Type { get; set; }
        public string Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? ImageUrl { get; set; }
        public string? ClipUrl { get; set; }
        public int? ReleaseYear { get; set; }
        public int? Runtime { get; set; }
        public long? LikeCount { get; set; }
        public long? DislikeCount { get; set; }
        public long? ImdbVotes { get; set; }
        public double? ImdbScore { get; set; }
        public double? TmdbPopularity { get; set; }
        public double? TmdbScore { get; set; }
        public int? TomatoMeter { get; set; }
        public double? JustWatchRating { get; set; }
        public string[] Languages { get; set; }
        public string[] Cast { get; set; }
        public List<CategoryResponse> Categories { get; set; }
        public List<ContentAvailabilityResponse> Availabilities { get; set; }
    }
}
