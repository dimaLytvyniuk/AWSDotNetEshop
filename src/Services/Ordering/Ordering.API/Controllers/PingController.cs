namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers;

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
