using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Samples.Db.WebApi.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return Redirect($"{Request.Scheme}://{Request.Host.ToUriComponent()}/swagger");
        }

        [HttpGet("health-check")]
        public IActionResult GetHealthCheck()
        {
            return StatusCode((int)HttpStatusCode.OK, new { MachineName = Environment.MachineName });
        }
    }
}