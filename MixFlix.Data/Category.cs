using System.ComponentModel.DataAnnotations;

namespace MixFlix.Data
{
    public class Category
    {
        public Guid Id { get; set; }

        [MaxLength(10)]
        public required string JustWatchId { get; set; }

        [MaxLength(10)]
        public string? TmdbId { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        public List<Content>? Contents { get; set; }
    }
}
