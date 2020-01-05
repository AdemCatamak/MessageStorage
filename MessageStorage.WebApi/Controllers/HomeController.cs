using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MessageStorage.WebApi.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return StatusCode((int) HttpStatusCode.OK, HttpStatusCode.OK.ToString());
        }
    }
}