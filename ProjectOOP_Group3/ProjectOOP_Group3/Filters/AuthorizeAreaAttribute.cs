using System;
using System.Web;
using System.Web.Mvc;

namespace ProjectOOP_Group3.Filters
{
    public class AuthorizeAreaAttribute : AuthorizeAttribute
    {
        public bool IsAdminArea { get; set; }

        public AuthorizeAreaAttribute(bool isAdminArea)
        {
            IsAdminArea = isAdminArea;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            // Lấy chức vụ từ session
            var chucVu = httpContext.Session["ChucVu"] as string;
            
            if (string.IsNullOrEmpty(chucVu))
            {
                return false;
            }

            // Kiểm tra quyền truy cập area
            bool isAdmin = chucVu == "Kế toán trưởng" || chucVu == "Admin";
            
            if (IsAdminArea)
            {
                // Chỉ Admin mới được vào Admin area
                return isAdmin;
            }
            else
            {
                // Chỉ User (không phải Admin) mới được vào User area
                return !isAdmin;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var chucVu = filterContext.HttpContext.Session["ChucVu"] as string;
            bool isAdmin = chucVu == "Kế toán trưởng" || chucVu == "Admin";
            
            string message = IsAdminArea 
                ? "Bạn không có quyền truy cập khu vực Admin. Chỉ Kế toán trưởng và Admin mới có quyền truy cập."
                : "Bạn không có quyền truy cập khu vực User. Vui lòng đăng nhập bằng tài khoản nhân viên.";

            filterContext.Result = new ViewResult
            {
                ViewName = "Unauthorized",
                ViewData = new ViewDataDictionary
                {
                    { "Message", message }
                }
            };
        }
    }
}

