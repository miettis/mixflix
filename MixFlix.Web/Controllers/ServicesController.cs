using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using MixFlix.Web.Models;

namespace MixFlix.Web.Controllers
{
    public class ServicesController : ApiController
    {
        public ServicesController(Context context) : base(context) { }

        // GET: api/services
        [HttpGet("")]
        [EndpointName("GetServices")]
        [Produces<ServiceResponse[]>]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await Context.Services.ToListAsync();
            var isLocal = IsLocal();
            var response = services
                .Select(s => new ServiceResponse{  Id = s.Id, Name = s.Name, Logo = s.Logo })
                .OrderBy(x => x.Name)
                .ToList();

            foreach(var service in response)
            {
                service.Logo = isLocal ? $"/src/assets/{service.Logo}" : $"/assets/{service.Logo}";
            }

            return Ok(response);
        }
    }
}
