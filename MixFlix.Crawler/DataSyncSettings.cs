namespace MixFlix.Crawler
{
    public class DataSyncSettings
    {
        public string SourceConnection { get; set; }
        public string TargetConnection { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RemoteDir { get; set; }
        public string LocalDir { get; set; }
    }
}
