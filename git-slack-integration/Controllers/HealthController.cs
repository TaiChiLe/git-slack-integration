using Microsoft.AspNetCore.Mvc;

namespace git_slack_integration.Controllers
{
    public class HealthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
