using System.Web.Mvc;

namespace DFramework.Pan.Web.Controllers
{
    public class AboutController : PanControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}