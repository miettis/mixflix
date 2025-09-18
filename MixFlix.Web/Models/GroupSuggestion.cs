using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class GroupSuggestion : ContentResponse
    {
        public int RatingCount { get; set; }
        public decimal TotalScore { get; set; }
        public decimal WeightedScore { get; set; }
    }
}
