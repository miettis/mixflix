using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using MixFlix.Web.Models;

namespace MixFlix.Web.Controllers
{
    public class CategoriesController : ApiController
    {
        public CategoriesController(Context context) : base(context) { }

        // GET: api/categories
        [HttpGet("")]
        [EndpointName("GetCategories")]
        [Produces<CategoryResponse[]>]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await Context.Categories
                .Select(x => new CategoryResponse { Id = x.Id, Name = x.Name})
                .OrderBy(x => x.Name)
                .ToListAsync();
            return Ok(categories);
        }
    }
}
