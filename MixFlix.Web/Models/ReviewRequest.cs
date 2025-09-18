using MixFlix.Data;
using System.ComponentModel.DataAnnotations;

namespace MixFlix.Web.Controllers
{
    public class ReviewRequest
    {
        [Required]
        public decimal Rating { get; set; }

        [Required]
        public Scale Type { get; set; }
    }
}
