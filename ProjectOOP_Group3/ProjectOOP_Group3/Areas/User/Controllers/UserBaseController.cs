using System.Web.Mvc;
using ProjectOOP_Group3.Filters;

namespace ProjectOOP_Group3.Areas.User.Controllers
{
    [Authorize]
    [AuthorizeArea(false)]
    public abstract class UserBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra session - phải có cả MaNV và ChucVu
            if (Session["MaNV"] == null || Session["ChucVu"] == null || string.IsNullOrEmpty(Session["MaNV"] as string) || string.IsNullOrEmpty(Session["ChucVu"] as string))
            {
                filterContext.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }

            var chucVu = Session["ChucVu"] as string;
            bool isAdmin = chucVu == "Kế toán trưởng" || chucVu == "Admin";

            if (isAdmin)
            {
                filterContext.Controller.TempData["ErrorMessage"] = "Bạn không có quyền truy cập khu vực User. Vui lòng sử dụng tài khoản Admin để truy cập Admin Panel.";
                filterContext.Result = RedirectToAction("Unauthorized", "Account", new { area = "" });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
        
        /// <summary>
        /// Hiển thị alert message thành công
        /// </summary>
        protected void SetSuccessAlert(string message, string title = "Thành công")
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = title;
        }
        
        /// <summary>
        /// Hiển thị alert message lỗi
        /// </summary>
        protected void SetErrorAlert(string message, string title = "Lỗi")
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = "error";
            TempData["AlertTitle"] = title;
        }
        
        /// <summary>
        /// Hiển thị alert message cảnh báo
        /// </summary>
        protected void SetWarningAlert(string message, string title = "Cảnh báo")
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = title;
        }
        
        /// <summary>
        /// Hiển thị alert message thông tin
        /// </summary>
        protected void SetInfoAlert(string message, string title = "Thông tin")
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = "info";
            TempData["AlertTitle"] = title;
        }
    }
}

