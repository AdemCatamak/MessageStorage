using Microsoft.AspNetCore.Mvc;

namespace AccountWebApi.Controllers
{
    public class HomeController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("")]
        public RedirectResult Home()
        {
            return Redirect($"{Request.Scheme}://{Request.Host.ToUriComponent()}/swagger");
        }
    }
}