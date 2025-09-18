namespace MixFlix.Data
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CreatorId { get; set; }
        public string Settings { get; set; }
        public bool? RequireApproval { get; set; }
        public User Creator { get; set; }
        public List<GroupMember> Members { get; set; }
    }
    public class GroupSettings
    {
        public HashSet<Guid>? Services { get; set; }
        public HashSet<ContentType> ContentTypes { get; set; }
        public HashSet<Guid>? Categories { get; set; }
        public GroupSettingsRange<int?>? ReleaseYear { get; set; }
        public GroupSettingsRange<long?>? ImdbVotes { get; set; }
        public GroupSettingsRange<double?>? ImdbScore { get; set; }
        public GroupSettingsRange<double?>? TmdbPopularity { get; set; }
        public GroupSettingsRange<double?>? TmdbScore { get; set; }
        public GroupSettingsRange<int?>? TomatoMeter { get; set; }
        public GroupSettingsRange<double?>? JustWatchRating { get; set; }
    }
    public class GroupSettingsRange<TNum>
    {
        public TNum Min { get; set; }
        public TNum Max { get; set; }
    }
}
