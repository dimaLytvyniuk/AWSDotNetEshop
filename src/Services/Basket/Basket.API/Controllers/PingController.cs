namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok("Test connection");
        }
    }
}
