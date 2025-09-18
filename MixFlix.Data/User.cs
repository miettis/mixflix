using System.ComponentModel.DataAnnotations;

namespace MixFlix.Data
{
    public class User
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public required string ExternalId { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool IsAdmin { get; set; }

        public List<GroupMember> Groups { get; set; }
    }
}
