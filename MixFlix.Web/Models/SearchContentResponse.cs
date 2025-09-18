using System.Collections.Generic;

namespace MixFlix.Web.Models
{
    public class SearchContentResponse
    {
        public IEnumerable<UserContentResponse> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
