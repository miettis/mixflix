namespace MixFlix.Web.Models
{
    public class MergeContentRequest
    {
        public Guid[] ContentIds { get; set; }
        public Guid DescriptionContentId { get; set; }
    }
}
