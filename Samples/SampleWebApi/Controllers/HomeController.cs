using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[Route("")]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    public IActionResult Home()
    {
        return Redirect($"{Request.Scheme}://{Request.Host.ToUriComponent()}/swagger");
    }
}