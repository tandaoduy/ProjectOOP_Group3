using System;
using System.Web;
using System.Web.Mvc;

namespace ProjectOOP_Group3.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public string[] AllowedRoles { get; set; }

        public AuthorizeRoleAttribute(params string[] roles)
        {
            AllowedRoles = roles;
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

            // Kiểm tra xem chức vụ có trong danh sách được phép không
            foreach (var role in AllowedRoles)
            {
                if (chucVu.Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new ViewResult
            {
                ViewName = "Unauthorized",
                ViewData = new ViewDataDictionary
                {
                    { "Message", "Bạn không có quyền truy cập trang này." }
                }
            };
        }
    }
}

