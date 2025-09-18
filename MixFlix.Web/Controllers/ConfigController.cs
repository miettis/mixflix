using Microsoft.AspNetCore.Mvc;

namespace MixFlix.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("client")]
        public IActionResult GetClientConfig()
        {
            var config = new
            {
                firebaseApiKey = _configuration["Firebase:ApiKey"] ?? "",
                firebaseAuthDomain = _configuration["Firebase:AuthDomain"] ?? "",
                firebaseProjectId = _configuration["Firebase:ProjectId"] ?? "",
                firebaseStorageBucket = _configuration["Firebase:StorageBucket"] ?? "",
                firebaseMessagingSenderId = _configuration["Firebase:MessagingSenderId"] ?? "",
                firebaseAppId = _configuration["Firebase:AppId"] ?? ""
            };

            return Ok(config);
        }
    }
}
