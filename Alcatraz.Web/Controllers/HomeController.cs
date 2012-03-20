using System.Web.Mvc;

namespace Alcatraz.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {         
            return View();
        }
    }
}
