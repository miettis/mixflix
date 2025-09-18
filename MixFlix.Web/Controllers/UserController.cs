using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixFlix.Data;
using MixFlix.Web.Models;
using System.Security.Claims;

namespace MixFlix.Web.Controllers
{
    public class UserController : ApiController
    {
        public UserController(Context context) : base(context) { }

        // POST: api/user/ensure
        [HttpPost("ensure")]
        [EndpointName("EnsureUser")]
        [Produces<UserResponse>]
        public async Task<IActionResult> EnsureUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var nameClaim = User.FindFirst(ClaimTypes.Name) ?? User.FindFirst("name");
            var emailClaim = User.FindFirst(ClaimTypes.Email);

            if (userIdClaim == null)
            {
                return BadRequest("User ID and Name are required.");
            }

            var existingUser = await Context.Users.FirstOrDefaultAsync(u => u.ExternalId == userIdClaim.Value);
            if (existingUser == null)
            {
                var user = new User 
                {
                    ExternalId = userIdClaim.Value, 
                    Name = nameClaim?.Value,
                    Email = emailClaim?.Value,
                };
                Context.Users.Add(user);
                await Context.SaveChangesAsync();
            }

            return Ok();
        }

        // GET: api/user/me
        [HttpGet("me")]
        [EndpointName("GetProfile")]
        [Produces<UserResponse>]
        public async Task<IActionResult> GetProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new UserResponse 
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsAdmin = user.IsAdmin
            });
        }

        // PUT: api/user/me
        [HttpPut("me")]
        [EndpointName("UpdateProfile")]

        public async Task<IActionResult> UpdateProfile([FromBody] UserRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            user.Name = request.Name;
            await Context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("me")]
        [EndpointName("DeleteProfile")]

        public async Task<IActionResult> DeleteProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            Context.Users.Remove(user);
            await Context.SaveChangesAsync();
            return Ok();
        }
    }
}
