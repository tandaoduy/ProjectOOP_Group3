using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Areas.Admin.Controllers
{
    public class DuyetPYCController : AdminBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: Admin/DuyetPYC - Danh sách phiếu yêu cầu chờ duyệt
        public ActionResult Index(int page = 1)
        {
            var phieuYeuCaus = db.PhieuYeuCaus
                .Where(p => p.TrangThai == "Chờ duyệt");
            
            // Phân trang: 5 items mỗi trang
            int pageSize = 5;
            int totalItems = phieuYeuCaus.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var pagedItems = phieuYeuCaus
                .OrderBy(p => p.NgayLap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedItems);
        }

        // GET: Admin/DuyetPYC/Details/5 - Xem chi tiết phiếu yêu cầu
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PhieuYeuCau phieuYeuCau = db.PhieuYeuCaus.Find(id);
            if (phieuYeuCau == null)
            {
                return HttpNotFound();
            }
            return View(phieuYeuCau);
        }

        // GET: Admin/DuyetPYC/Duyet/5 - Form duyệt phiếu
        public ActionResult Duyet(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PhieuYeuCau phieuYeuCau = db.PhieuYeuCaus.Find(id);
            if (phieuYeuCau == null || phieuYeuCau.TrangThai != "Chờ duyệt")
            {
                return HttpNotFound();
            }
            return View(phieuYeuCau);
        }

        // POST: Admin/DuyetPYC/Duyet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Duyet(string id, string trangThai, string ghiChuDuyet)
        {
            PhieuYeuCau phieuYeuCau = db.PhieuYeuCaus.Find(id);
            if (phieuYeuCau == null)
            {
                return HttpNotFound();
            }

            // Lấy MaNV từ session
            string maNV_Duyet = Session["MaNV"] as string;
            
            // Kiểm tra xem MaNV có tồn tại trong database không
            if (string.IsNullOrEmpty(maNV_Duyet))
            {
                ViewBag.AlertMessage = "Không tìm thấy thông tin nhân viên. Vui lòng đăng nhập lại.";
                ViewBag.AlertType = "error";
                ViewBag.AlertTitle = "Lỗi xác thực";
                return View(phieuYeuCau);
            }

            // Kiểm tra nhân viên có tồn tại trong database
            var nhanVien = db.NhanViens.FirstOrDefault(n => n.MaNV == maNV_Duyet);
            if (nhanVien == null)
            {
                ViewBag.AlertMessage = "Mã nhân viên không hợp lệ. Vui lòng đăng nhập lại.";
                ViewBag.AlertType = "error";
                ViewBag.AlertTitle = "Lỗi xác thực";
                return View(phieuYeuCau);
            }

            if (ModelState.IsValid)
            {
                phieuYeuCau.TrangThai = trangThai;
                phieuYeuCau.NgayDuyet = DateTime.Now;
                phieuYeuCau.GhiChuDuyet = ghiChuDuyet;
                phieuYeuCau.MaNV_Duyet = maNV_Duyet;

                try
                {
                    db.SaveChanges();
                    string successMessage = trangThai == "Đã duyệt" 
                        ? $"Đã duyệt thành công phiếu yêu cầu <strong>{phieuYeuCau.MaPYC}</strong>." 
                        : $"Đã từ chối phiếu yêu cầu <strong>{phieuYeuCau.MaPYC}</strong>.";
                    SetSuccessAlert(successMessage, "Duyệt thành công");
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    string errorMessage = "Lỗi validation dữ liệu: ";
                    var validationErrors = new List<string>();
                    
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        foreach (var error in validationError.ValidationErrors)
                        {
                            validationErrors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                        }
                    }
                    
                    if (validationErrors.Any())
                    {
                        errorMessage += string.Join("; ", validationErrors);
                    }
                    else
                    {
                        errorMessage += ex.Message;
                    }
                    
                    ViewBag.AlertMessage = errorMessage;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi validation";
                    return View(phieuYeuCau);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    string errorMessage = "Lỗi khi lưu dữ liệu. ";
                    
                    // Lấy inner exception chi tiết
                    Exception innerEx = ex.InnerException;
                    while (innerEx != null && innerEx.InnerException != null)
                    {
                        innerEx = innerEx.InnerException;
                    }
                    
                    if (innerEx != null)
                    {
                        var sqlException = innerEx as System.Data.SqlClient.SqlException;
                        if (sqlException != null)
                        {
                            switch (sqlException.Number)
                            {
                                case 547: // Foreign key constraint
                                    errorMessage += "Dữ liệu không hợp lệ: Có tham chiếu đến dữ liệu không tồn tại. Vui lòng kiểm tra lại thông tin nhập vào.";
                                    break;
                                case 2627: // Unique constraint
                                    errorMessage += "Dữ liệu trùng lặp: Mã đã tồn tại trong hệ thống.";
                                    break;
                                case 515: // Cannot insert NULL
                                    errorMessage += "Thiếu thông tin bắt buộc: Vui lòng điền đầy đủ các trường bắt buộc.";
                                    break;
                                default:
                                    errorMessage += sqlException.Message;
                                    break;
                            }
                        }
                        else
                        {
                            errorMessage += innerEx.Message;
                        }
                    }
                    else
                    {
                        errorMessage += ex.Message;
                    }
                    
                    if (ex.InnerException != null)
                    {
                        errorMessage += " Chi tiết: " + ex.InnerException.Message;
                    }
                    
                    ViewBag.AlertMessage = errorMessage;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi khi lưu dữ liệu";
                    return View(phieuYeuCau);
                }
            }

            return View(phieuYeuCau);
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

