using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using System.Security.Claims;

namespace MixFlix.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class ApiController : ControllerBase
    {
        protected readonly Context Context;

        // Constructor that takes Context
        protected ApiController(Context context)
        {
            Context = context;
        }

        // Helper: Get current user ExternalId from claims
        protected string? GetCurrentUserExternalId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Helper: Get current user from DB
        protected async Task<User?> GetCurrentUserAsync()
        {
            return await Context.Users.FirstOrDefaultAsync(u => u.ExternalId == GetCurrentUserExternalId());
        }

        protected string GetContentImageUrl(Guid contentId, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            var isLocal = Request.Host.Host.Contains("localhost");

            if (path.StartsWith("/poster"))
            {

                return isLocal ?
                    $"https://{Request.Host.Host}:{Request.Host.Port}/api/content/images/{contentId}_md.avif" :
                    $"/api/content/images/{contentId}_md.avif";
            }
            else
            {
                return isLocal ?
                    $"https://{Request.Host.Host}:{Request.Host.Port}/api/content/images/{contentId}.jpg" :
                    $"/api/content/images/{contentId}.jpg";
            }
        }

        protected bool IsLocal()
        {
            return Request.Host.Host.Contains("localhost");
        }
    }
}
