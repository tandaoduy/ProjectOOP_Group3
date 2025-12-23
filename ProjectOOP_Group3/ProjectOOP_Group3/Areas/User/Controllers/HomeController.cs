using System.Web.Mvc;

namespace ProjectOOP_Group3.Areas.User.Controllers
{
    public class HomeController : UserBaseController
    {
        // GET: User/Home
        public ActionResult Index()
        {
            // Kiểm tra session - nếu chưa đăng nhập thì redirect về login (kiểm tra kỹ)
            if (Session["MaNV"] == null || Session["ChucVu"] == null || 
                string.IsNullOrEmpty(Session["MaNV"] as string) || 
                string.IsNullOrEmpty(Session["ChucVu"] as string))
            {
                // Xóa session nếu không hợp lệ
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Kiểm tra quyền - nếu là Admin thì không được vào User area
            var chucVu = Session["ChucVu"] as string;
            bool isAdmin = chucVu == "Kế toán trưởng" || chucVu == "Admin";
            
            if (isAdmin)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return View();
        }
    }
}

