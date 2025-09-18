using System.Text.Json.Serialization;

namespace StreamBuddy.Crawler.PrimeVideo
{
    public partial class Page
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("assembly")]
        public Assembly Assembly { get; set; }
    }

    public partial class Assembly
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("body")]
        public Body[] Body { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("metadata")]
        public AssemblyMetadata Metadata { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("appArgs")]
        public AppArgs AppArgs { get; set; }
    }

    public partial class AppArgs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("context")]
        public Context Context { get; set; }
    }

    public partial class AssemblyMetadata
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("pageType")]
        public string PageType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("subPageType")]
        public string SubPageType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("pageTitle")]
        public string PageTitle { get; set; }
    }
}
