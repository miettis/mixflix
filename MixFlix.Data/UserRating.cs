namespace MixFlix.Data
{
    public class UserRating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ContentId { get; set; }
        public Scale Type { get; set; }
        public decimal Rating { get; set; }
        public DateTimeOffset? RatingTime { get; set; }
        public User User { get; set; }
        public Content Content { get; set; }
    }
    public enum Scale
    {
        None,
        FivePoint
    }
}
