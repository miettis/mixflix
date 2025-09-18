namespace MixFlix.Data
{
    public class ContentAvailability
    {
        public long Id { get; set; }

        public Guid ContentId { get; set; }
        public Guid ServiceId { get; set; }

        public DateTimeOffset? JustWatchLastSeen { get; set; }
        public int? JustWatchRanking { get; set; }

        public DateTimeOffset? TmdbLastSeen { get; set; }
        public int? TmdbRanking { get; set; }

        public Content Content { get; set; }
        public Service Service { get; set; }
    }
}
