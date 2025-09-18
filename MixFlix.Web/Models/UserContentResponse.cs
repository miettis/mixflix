using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class UserContentResponse : ContentResponse
    {
        public decimal? UserRating { get; set; }
        public DateTimeOffset? UserRatingTime { get; set; }
    }
}
