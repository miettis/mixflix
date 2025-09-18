using System.Text.Json.Serialization;

namespace MixFlix.Data
{
    public class GroupMember
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public GroupMemberStatus Status { get; set; }
        public Group Group { get; set; }
        public User User { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupMemberStatus
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Pending approval
        /// </summary>
        Pending,
        /// <summary>
        /// Approved member
        /// </summary>
        Approved,
        /// <summary>
        /// Rejected member
        /// </summary>
        Rejected
    }
}
