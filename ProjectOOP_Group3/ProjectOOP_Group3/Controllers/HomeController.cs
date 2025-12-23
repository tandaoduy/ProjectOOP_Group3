using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectOOP_Group3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Nếu chưa đăng nhập, redirect về Login
            if (Session["MaNV"] == null || Session["ChucVu"] == null || 
                string.IsNullOrEmpty(Session["MaNV"] as string) || 
                string.IsNullOrEmpty(Session["ChucVu"] as string))
            {
                // Xóa session nếu không hợp lệ
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Account");
            }
            
            // Kiểm tra nếu đã đăng nhập, redirect đến area tương ứng
            var chucVu = Session["ChucVu"] as string;
            bool isAdmin = chucVu == "Kế toán trưởng" || chucVu == "Admin";
            
            if (isAdmin)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "User" });
            }
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}