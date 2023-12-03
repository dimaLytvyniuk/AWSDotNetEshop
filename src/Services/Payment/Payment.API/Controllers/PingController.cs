using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.eShopOnContainers.Services.Payment.API.Controllers;

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
