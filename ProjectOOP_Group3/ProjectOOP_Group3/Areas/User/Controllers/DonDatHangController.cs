using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Modal;
using ProjectOOP_Group3.Areas.User.Models;

namespace ProjectOOP_Group3.Areas.User.Controllers
{
    public class DonDatHangController : UserBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: User/DonDatHang - Danh sách đơn đặt hàng
        public ActionResult Index(int page = 1)
        {
            var donDatHangs = db.DonDatHangs.Include("PhieuYeuCau").Include("NhaCungCap");
            
            // Phân trang: 5 items mỗi trang
            int pageSize = 5;
            int totalItems = donDatHangs.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var pagedItems = donDatHangs
                .OrderByDescending(d => d.NgayLap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedItems);
        }

        // GET: User/DonDatHang/ChonPhieuYeuCau - Chọn phiếu yêu cầu để tạo đơn đặt hàng
        public ActionResult ChonPhieuYeuCau(int page = 1)
        {
            // Lấy danh sách mã PYC đã có đơn đặt hàng
            var maPYCDaCoDDH = db.DonDatHangs
                .Where(d => d.MaPYC != null)
                .Select(d => d.MaPYC)
                .ToList();

            // Hiển thị danh sách phiếu yêu cầu đã duyệt và chưa có đơn đặt hàng
            var phieuYeuCaus = db.PhieuYeuCaus
                .Include("BoPhan")
                .Where(p => p.TrangThai == "Đã duyệt" && (p.MaPYC == null || !maPYCDaCoDDH.Contains(p.MaPYC)))
                .OrderByDescending(p => p.NgayDuyet ?? p.NgayLap);

            // Phân trang: 5 items mỗi trang
            if (page < 1) page = 1;
            
            int pageSize = 5;
            int totalItems = phieuYeuCaus.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var pagedItems = phieuYeuCaus
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedItems);
        }

        // GET: User/DonDatHang/Create - Tạo đơn đặt hàng từ phiếu yêu cầu đã duyệt
        public ActionResult Create(string maPYC)
        {
            if (String.IsNullOrEmpty(maPYC))
            {
                // Lấy danh sách mã PYC đã có đơn đặt hàng
                var maPYCDaCoDDH = db.DonDatHangs
                    .Where(d => d.MaPYC != null)
                    .Select(d => d.MaPYC)
                    .ToList();

                // Hiển thị danh sách phiếu yêu cầu đã duyệt và chưa có đơn đặt hàng
                var phieuYeuCaus = db.PhieuYeuCaus
                    .Include("BoPhan")
                    .Where(p => p.TrangThai == "Đã duyệt" && (p.MaPYC == null || !maPYCDaCoDDH.Contains(p.MaPYC)))
                    .OrderByDescending(p => p.NgayDuyet ?? p.NgayLap)
                    .ToList();
                return View("ChonPhieuYeuCau", phieuYeuCaus);
            }

            var phieuYeuCau = db.PhieuYeuCaus
                .Include("ChiTietPYCs")
                .FirstOrDefault(p => p.MaPYC == maPYC);
            
            if (phieuYeuCau == null || phieuYeuCau.TrangThai != "Đã duyệt")
            {
                return HttpNotFound();
            }

            // Phân nhóm chi tiết PYC theo nhà cung cấp
            var chiTietPYCs = phieuYeuCau.ChiTietPYCs.ToList();
            
            // Xử lý chi tiết không có MaNCC_Gán - thử lấy từ BangGiaNhaCungCap
            foreach (var chiTiet in chiTietPYCs.Where(c => string.IsNullOrEmpty(c.MaNCC_Gán)))
            {
                if (!string.IsNullOrEmpty(chiTiet.MaMH))
                {
                    var bangGia = db.BangGiaNhaCungCaps
                        .Where(b => b.MaMH == chiTiet.MaMH)
                        .OrderByDescending(b => b.NgayCapNhat)
                        .FirstOrDefault();
                    
                    if (bangGia != null && !string.IsNullOrEmpty(bangGia.MaNCC))
                    {
                        chiTiet.MaNCC_Gán = bangGia.MaNCC;
                    }
                }
            }
            
            // Nhóm các chi tiết theo MaNCC_Gán
            var nccGroups = chiTietPYCs
                .Where(c => !string.IsNullOrEmpty(c.MaNCC_Gán))
                .GroupBy(c => c.MaNCC_Gán)
                .ToList();

            // Tạo danh sách nhà cung cấp với thông tin chi tiết
            var nhaCungCapList = new List<NhaCungCapInfoViewModel>();
            foreach (var group in nccGroups)
            {
                var ncc = db.NhaCungCaps.Find(group.Key);
                if (ncc != null)
                {
                    var chiTietList = group.ToList();
                    nhaCungCapList.Add(new NhaCungCapInfoViewModel
                    {
                        MaNCC = ncc.MaNCC,
                        TenNCC = ncc.TenNCC,
                        ChiTietPYCs = chiTietList,
                        SoLuongMatHang = chiTietList.Count,
                        TongTien = chiTietList.Sum(c => (c.DonGiaDuKien ?? 0) * c.SoLuong)
                    });
                }
            }

            // Tạo model DonDatHang với MaPYC (MaNCC sẽ được set khi tạo đơn)
            var donDatHang = new DonDatHang
            {
                MaPYC = maPYC
            };

            ViewBag.NhaCungCapList = nhaCungCapList;
            ViewBag.ChiTietPYC = chiTietPYCs;
            ViewBag.SoLuongNCC = nhaCungCapList.Count;
            
            return View(donDatHang);
        }

        // POST: User/DonDatHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DonDatHang donDatHang)
        {
            // Validation: Ngày giao dự kiến phải >= ngày hiện tại và <= 5 năm từ ngày hiện tại
            if (donDatHang.NgayGiaoDuKien.HasValue)
            {
                var today = DateTime.Now.Date;
                var maxDate = today.AddYears(5);
                
                if (donDatHang.NgayGiaoDuKien.Value.Date < today)
                {
                    ModelState.AddModelError("NgayGiaoDuKien", "Ngày giao dự kiến không thể là ngày trong quá khứ");
                }
                else if (donDatHang.NgayGiaoDuKien.Value.Date > maxDate)
                {
                    ModelState.AddModelError("NgayGiaoDuKien", "Ngày giao dự kiến không được vượt quá 5 năm từ ngày hiện tại");
                }
            }
            
            if (ModelState.IsValid)
            {
                var phieuYeuCau = db.PhieuYeuCaus
                    .Include("ChiTietPYCs")
                    .FirstOrDefault(p => p.MaPYC == donDatHang.MaPYC);
                
                if (phieuYeuCau == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy phiếu yêu cầu");
                    ViewBag.ChiTietPYC = new List<ChiTietPYC>();
                    return View(donDatHang);
                }

                // Phân nhóm chi tiết PYC theo nhà cung cấp
                var chiTietPYCs = phieuYeuCau.ChiTietPYCs.ToList();
                
                // Xử lý chi tiết không có MaNCC_Gán - thử lấy từ BangGiaNhaCungCap
                foreach (var chiTiet in chiTietPYCs.Where(c => string.IsNullOrEmpty(c.MaNCC_Gán)))
                {
                    if (!string.IsNullOrEmpty(chiTiet.MaMH))
                    {
                        var bangGia = db.BangGiaNhaCungCaps
                            .Where(b => b.MaMH == chiTiet.MaMH)
                            .OrderByDescending(b => b.NgayCapNhat)
                            .FirstOrDefault();
                        
                        if (bangGia != null && !string.IsNullOrEmpty(bangGia.MaNCC))
                        {
                            chiTiet.MaNCC_Gán = bangGia.MaNCC;
                        }
                    }
                }
                
                // Nhóm các chi tiết theo MaNCC_Gán
                var nccGroups = chiTietPYCs
                    .Where(c => !string.IsNullOrEmpty(c.MaNCC_Gán))
                    .GroupBy(c => c.MaNCC_Gán)
                    .ToList();

                if (!nccGroups.Any())
                {
                    ModelState.AddModelError("", "Không tìm thấy nhà cung cấp cho các mặt hàng trong phiếu yêu cầu");
                    ViewBag.ChiTietPYC = chiTietPYCs;
                    return View(donDatHang);
                }

                try
                {
                    // Tạo mã DDH tự động - đảm bảo unique
                    var existingMaDDHs = db.DonDatHangs.Select(d => d.MaDDH).ToList();
                    var lastDDH = db.DonDatHangs.OrderByDescending(d => d.MaDDH).FirstOrDefault();
                    int nextNumber = 1;
                    if (lastDDH != null)
                    {
                        var lastNumber = lastDDH.MaDDH.Substring(3);
                        if (int.TryParse(lastNumber, out int num))
                        {
                            nextNumber = num + 1;
                        }
                    }

                    var createdOrders = new List<string>();
                    
                    // Tạo đơn hàng cho mỗi nhà cung cấp
                    foreach (var nccGroup in nccGroups)
                    {
                        // Tìm mã DDH chưa tồn tại
                        string newMaDDH = null;
                        int currentNumber = nextNumber;
                        
                        while (newMaDDH == null)
                        {
                            string candidateMaDDH = "DDH" + currentNumber.ToString("D4");
                            if (!existingMaDDHs.Contains(candidateMaDDH))
                            {
                                newMaDDH = candidateMaDDH;
                                existingMaDDHs.Add(candidateMaDDH);
                            }
                            else
                            {
                                currentNumber++;
                            }
                        }

                        // Tính tổng tiền cho đơn hàng này
                        decimal tongTien = 0;
                        foreach (var chiTiet in nccGroup)
                        {
                            if (chiTiet.DonGiaDuKien.HasValue)
                            {
                                tongTien += chiTiet.DonGiaDuKien.Value * chiTiet.SoLuong;
                            }
                        }

                        // Tạo đơn đặt hàng
                        var donDatHangMoi = new DonDatHang
                        {
                            MaDDH = newMaDDH,
                            MaPYC = donDatHang.MaPYC,
                            MaNCC = nccGroup.Key,
                            NgayLap = DateTime.Now,
                            NgayGiaoDuKien = donDatHang.NgayGiaoDuKien,
                            DieuKhoan = donDatHang.DieuKhoan,
                            TongTien = tongTien
                        };

                        db.DonDatHangs.Add(donDatHangMoi);
                        createdOrders.Add(newMaDDH);
                        
                        nextNumber = currentNumber + 1;
                    }

                    db.SaveChanges();
                    
                    string successMessage = createdOrders.Count > 1 
                        ? $"Đã tạo thành công {createdOrders.Count} đơn đặt hàng: {string.Join(", ", createdOrders)}"
                        : $"Đơn đặt hàng {createdOrders.First()} đã được tạo thành công!";
                    
                    SetSuccessAlert(successMessage, "Thành công");
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
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", donDatHang.MaNCC);
                    if (!String.IsNullOrEmpty(donDatHang.MaPYC))
                    {
                        var phieuYeuCauForView = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                        ViewBag.ChiTietPYC = phieuYeuCauForView?.ChiTietPYCs.ToList();
                    }
                    return View(donDatHang);
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
                                    errorMessage += "Dữ liệu trùng lặp: Mã đơn đặt hàng đã tồn tại trong hệ thống.";
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
                    
                    ViewBag.AlertMessage = errorMessage;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi khi lưu dữ liệu";
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", donDatHang.MaNCC);
                    if (!String.IsNullOrEmpty(donDatHang.MaPYC))
                    {
                        var phieuYeuCauForView = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                        ViewBag.ChiTietPYC = phieuYeuCauForView?.ChiTietPYCs.ToList();
                    }
                    return View(donDatHang);
                }
                catch (Exception ex)
                {
                    ViewBag.AlertMessage = "Lỗi không xác định: " + ex.Message;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi";
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", donDatHang.MaNCC);
                    if (!String.IsNullOrEmpty(donDatHang.MaPYC))
                    {
                        var phieuYeuCauForView = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                        ViewBag.ChiTietPYC = phieuYeuCauForView?.ChiTietPYCs.ToList();
                    }
                    return View(donDatHang);
                }
            }

            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", donDatHang.MaNCC);
            if (!String.IsNullOrEmpty(donDatHang.MaPYC))
            {
                var phieuYeuCauForValidation = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                ViewBag.ChiTietPYC = phieuYeuCauForValidation?.ChiTietPYCs.ToList();
            }
            return View(donDatHang);
        }

        // GET: User/DonDatHang/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            DonDatHang donDatHang = db.DonDatHangs
                .Include("PhieuYeuCau")
                .Include("PhieuYeuCau.BoPhan")
                .Include("PhieuYeuCau.ChiTietPYCs")
                .Include("PhieuYeuCau.ChiTietPYCs.MatHang")
                .Include("NhaCungCap")
                .FirstOrDefault(d => d.MaDDH == id);
            if (donDatHang == null)
            {
                return HttpNotFound();
            }
            return View(donDatHang);
        }

        // GET: User/DonDatHang/Print/5 - In đơn đặt hàng
        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            DonDatHang donDatHang = db.DonDatHangs
                .Include("PhieuYeuCau")
                .Include("PhieuYeuCau.BoPhan")
                .Include("PhieuYeuCau.ChiTietPYCs")
                .Include("PhieuYeuCau.ChiTietPYCs.MatHang")
                .Include("NhaCungCap")
                .FirstOrDefault(d => d.MaDDH == id);
            if (donDatHang == null)
            {
                return HttpNotFound();
            }
            return View(donDatHang);
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

