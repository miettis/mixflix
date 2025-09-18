using MixFlix.Data;
using System.Text.Json.Serialization;

namespace MixFlix.Web.Models
{
    public class GroupResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CreatorId { get; set; }
        public GroupSettings Settings { get; set; }
        public int MemberCount { get; set; }
        [JsonIgnore]
        public string? SettingsJson { get; set; }
        public bool RequireApproval { get; set; }
        public bool IsMember { get; set; }
        public bool IsCreator { get; set; }
        public string ShareLink { get; set; }
        public List<MemberResponse> Members { get; set; }
    }
    public class MemberResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public GroupMemberStatus Status { get; set; }
    }
}
