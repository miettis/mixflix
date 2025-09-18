namespace MixFlix.Web.Models
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool IsAdmin { get; set; }
    }
}
