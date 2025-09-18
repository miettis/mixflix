using System.Text.Json.Serialization;

namespace StreamBuddy.Crawler.PrimeVideo
{
    public partial class Context
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customerID")]
        public string CustomerId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isInternal")]
        public bool? IsInternal { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("queryParameters")]
        public ContextQueryParameters QueryParameters { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("requestID")]
        public string RequestId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("sessionID")]
        public string SessionId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("trafficPolicies")]
        public string TrafficPolicies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("marketplaceID")]
        public string MarketplaceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customerIPAddress")]
        public string CustomerIpAddress { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("originalURI")]
        public string OriginalUri { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("osLocale")]
        public string OsLocale { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("recordTerritory")]
        public string RecordTerritory { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("currentTerritory")]
        public string CurrentTerritory { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("geoToken")]
        public string GeoToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("cookieTimezone")]
        public string CookieTimezone { get; set; }

        [JsonPropertyName("appName")]
        public object AppName { get; set; }

        [JsonPropertyName("deviceID")]
        public object DeviceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("contingencies")]
        public Contingencies Contingencies { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isTest")]
        public bool? IsTest { get; set; }

        [JsonPropertyName("mocks")]
        public object Mocks { get; set; }

        [JsonPropertyName("serviceOverrides")]
        public object ServiceOverrides { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isLocaleRTL")]
        public bool? IsLocaleRtl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("identityContext")]
        public string IdentityContext { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        [JsonPropertyName("resiliencyToken")]
        public object ResiliencyToken { get; set; }
    }

    public partial class Contingencies
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isTesting")]
        public bool? IsTesting { get; set; }
    }

    public partial class ContextQueryParameters
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("dvWebSPAClientVersion")]
        public string[] DvWebSpaClientVersion { get; set; }
    }
}
