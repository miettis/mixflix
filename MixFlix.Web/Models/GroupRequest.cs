using MixFlix.Data;

namespace MixFlix.Web.Models
{
    public class GroupRequest
    {
        public string Name { get; set; }
        public bool RequireApproval { get; set; }
        public GroupSettings? Settings { get; set; }
    }
}
