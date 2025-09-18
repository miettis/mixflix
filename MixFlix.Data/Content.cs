using System.ComponentModel.DataAnnotations;

namespace MixFlix.Data
{
    public class Content
    {
        public Guid Id { get; set; }

        [MaxLength(20)]
        public string? JustWatchId { get; set; }

        [MaxLength(20)]
        public string? TmdbId { get; set; }

        [MaxLength(20)]
        public string? ImdbId { get; set; }

        public ContentType Type { get; set; }

        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(200)]
        public string? TitleEn { get; set; }

        public string? ShortDescription { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        [MaxLength(200)]
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

        [MaxLength(20)]
        public string? Language { get; set; }

        public string? Cast { get; set; }

        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Modified { get; set; }

        public List<Category>? Categories { get; set; }
        public List<ContentAvailability> Availabilities { get; set; }
    }
}
