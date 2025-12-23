using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Controllers
{
    public class AccountController : Controller
    {
        private Modal.ProjectOOP_Nhom3Entities1 db = new Modal.ProjectOOP_Nhom3Entities1();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, redirect đến area tương ứng
            if (Session["ChucVu"] != null)
            {
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
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string maNV, string matKhau, string returnUrl)
        {
            if (string.IsNullOrEmpty(maNV) || string.IsNullOrEmpty(matKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập mã nhân viên và mật khẩu.");
                return View();
            }

            // Tìm nhân viên trong database
            var nhanVien = db.NhanViens.FirstOrDefault(n => n.MaNV == maNV);

            if (nhanVien == null)
            {
                ModelState.AddModelError("", "Mã nhân viên hoặc mật khẩu không đúng.");
                return View();
            }

            // Kiểm tra mật khẩu (so sánh trực tiếp - trong thực tế nên hash password)
            if (nhanVien.MatKhau != matKhau)
            {
                ModelState.AddModelError("", "Mã nhân viên hoặc mật khẩu không đúng.");
                return View();
            }

            // Kiểm tra trạng thái tài khoản
            if (nhanVien.TrangThai == false)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View();
            }

            // Kiểm tra quyền truy cập nếu có returnUrl
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                bool isAdmin = nhanVien.ChucVu == "Kế toán trưởng" || nhanVien.ChucVu == "Admin";
                bool isAdminArea = returnUrl.Contains("/Admin/");
                bool isUserArea = returnUrl.Contains("/User/");

                // Nếu user cố truy cập Admin area nhưng không phải Admin
                if (isAdminArea && !isAdmin)
                {
                    ModelState.AddModelError("", "Bạn không có quyền truy cập khu vực Admin. Chỉ Kế toán trưởng và Admin mới có quyền truy cập.");
                    return View();
                }

                // Nếu Admin cố truy cập User area
                if (isUserArea && isAdmin)
                {
                    ModelState.AddModelError("", "Bạn không có quyền truy cập khu vực User. Vui lòng sử dụng tài khoản Admin để truy cập Admin Panel.");
                    return View();
                }
            }

            // Tạo Forms Authentication Ticket
            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: nhanVien.MaNV,
                issueDate: DateTime.Now,
                expiration: DateTime.Now.AddMinutes(30), // 30 phút
                isPersistent: false,
                userData: nhanVien.ChucVu // Lưu chức vụ để phân quyền
            );

            // Mã hóa ticket
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // Tạo cookie
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (ticket.IsPersistent)
            {
                authCookie.Expires = ticket.Expiration;
            }

            Response.Cookies.Add(authCookie);

            // Lưu thông tin nhân viên vào session
            Session["MaNV"] = nhanVien.MaNV;
            Session["HoTen"] = nhanVien.HoTen;
            Session["ChucVu"] = nhanVien.ChucVu;
            Session["Email"] = nhanVien.Email;

            // Redirect về URL ban đầu hoặc trang chủ
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Phân quyền redirect
            if (nhanVien.ChucVu == "Kế toán trưởng" || nhanVien.ChucVu == "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "User" });
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Unauthorized
        [AllowAnonymous]
        public ActionResult Unauthorized(string message)
        {
            ViewBag.Message = TempData["ErrorMessage"] as string ?? message ?? "Bạn không có quyền truy cập trang này.";
            return View("~/Views/Shared/Unauthorized.cshtml");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

