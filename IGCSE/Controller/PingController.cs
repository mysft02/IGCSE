using Microsoft.AspNetCore.Mvc;

namespace IGCSE.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PingController> _logger;

        public PingController(IConfiguration config, ILogger<PingController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet(Name = "GetPing")]
        public IActionResult Get()
        {
            _logger.LogInformation("Ping");
            var appVer = Environment.GetEnvironmentVariable("APP_VERSION");

            return Ok("Ok");
        }
    }
}
