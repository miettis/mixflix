namespace MixFlix.Web.Models
{
    public class GroupSuggestionsResponse
    {
        public IEnumerable<GroupSuggestion> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
