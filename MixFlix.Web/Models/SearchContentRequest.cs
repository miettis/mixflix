using System.Text.Json.Serialization;
using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class SearchContentRequest
    {
        public HashSet<Guid>? Services { get; set; }
        public HashSet<ContentType>? ContentTypes { get; set; }
        public HashSet<Guid>? Categories { get; set; }
        public bool? Reviewed { get; set; }
        public bool? NotReviewed { get; set; }
        public decimal[]? Ratings { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContentSorting SortBy { get; set; }
        public int? PageSize { get; set; }
        public int? Offset { get; set; }
        /// <summary>
        /// Optional: filter by text contained in the title (case-insensitive)
        /// </summary>
        public string? Title { get; set; }
    }

    /// <summary>
    /// Sorting options for content search.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentSorting
    {
        /// <summary>No sorting</summary>
        None,
        /// <summary>Sort alphabetically</summary>
        Alphabetical,
        /// <summary>Sort by popularity</summary>
        MostPopular,
        /// <summary>Sort by year ascending</summary>
        YearAscending,
        /// <summary>Sort by year descending</summary>
        YearDescending,
        /// <summary>Sort by IMDb rating</summary>
        ImdbRating,
        /// <summary>Sort by TMDb rating</summary>
        TmdbRating,
        /// <summary>Sort by my rating ascending</summary>
        MyRatingAscending,
        /// <summary>Sort by my rating descending</summary>
        MyRatingDescending,
        /// <summary>Sort by my rating time ascending</summary>
        MyRatingTimeAscending,
        /// <summary>Sort by my rating time descending</summary>
        MyRatingTimeDescending
    }
}
