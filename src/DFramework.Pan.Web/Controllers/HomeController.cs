using System.Web.Mvc;

namespace DFramework.Pan.Web.Controllers
{
    public class HomeController : PanControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}