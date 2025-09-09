using Microsoft.AspNetCore.Mvc;

namespace git_slack_integration.Controllers
{
    public class SlackController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
