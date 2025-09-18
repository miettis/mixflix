using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class PossibleDuplicateResponse
    {
        public ContentType Type { get; set; }
        public string Title { get; set; }
        public int? ReleaseYear { get; set; }
        public List<PossibleDuplicateItem> Items { get; set; }

    }
    public class PossibleDuplicateItem
    {
        public Guid Id { get; set; }      
        public string? ShortDescription { get; set; }
        public string? ImageUrl { get; set; }
    }
}
