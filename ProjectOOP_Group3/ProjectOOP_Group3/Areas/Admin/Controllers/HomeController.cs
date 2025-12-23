using System.Web.Mvc;

namespace ProjectOOP_Group3.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}

