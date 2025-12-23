using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Areas.User.Controllers
{
    public class PhieuYeuCauManualController : UserBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: User/PhieuYeuCauManual/Create
        public ActionResult Create()
        {
            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
            return View();
        }

        // POST: User/PhieuYeuCauManual/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuYeuCau phieuYeuCau, string[] TenMH, string[] DonViTinh, int[] SoLuong, string[] TenNCC, string[] DiaChiNCC, string[] SoDTNCC, decimal[] DonGiaDuKien)
        {
            if (ModelState.IsValid)
            {
                // Tạo mã PYC tự động - đảm bảo unique
                string newMaPYC = null;
                int nextNumber = 1;
                
                // Lấy tất cả các mã PYC hiện có
                var existingMaPYCs = db.PhieuYeuCaus.Select(p => p.MaPYC).ToList();
                
                // Tìm mã lớn nhất
                var lastPYC = db.PhieuYeuCaus.OrderByDescending(p => p.MaPYC).FirstOrDefault();
                if (lastPYC != null)
                {
                    var lastNumber = lastPYC.MaPYC.Substring(3);
                    if (int.TryParse(lastNumber, out int num))
                    {
                        nextNumber = num + 1;
                    }
                }
                
                // Tìm mã chưa tồn tại
                while (newMaPYC == null)
                {
                    string candidateMaPYC = "PYC" + nextNumber.ToString("D4");
                    if (!existingMaPYCs.Contains(candidateMaPYC))
                    {
                        newMaPYC = candidateMaPYC;
                    }
                    else
                    {
                        nextNumber++;
                    }
                }
                
                phieuYeuCau.MaPYC = newMaPYC;

                phieuYeuCau.NgayLap = DateTime.Now;
                phieuYeuCau.TrangThai = "Chờ duyệt";
                
                // Lấy MaNV_Lap từ session
                string maNV_Lap = Session["MaNV"] as string;
                if (string.IsNullOrEmpty(maNV_Lap))
                {
                    ViewBag.AlertMessage = "Không tìm thấy thông tin nhân viên. Vui lòng đăng nhập lại.";
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi xác thực";
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
                
                // Kiểm tra nhân viên có tồn tại trong database
                var nhanVien = db.NhanViens.FirstOrDefault(n => n.MaNV == maNV_Lap);
                if (nhanVien == null)
                {
                    ViewBag.AlertMessage = "Mã nhân viên không hợp lệ. Vui lòng đăng nhập lại.";
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi xác thực";
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
                
                phieuYeuCau.MaNV_Lap = maNV_Lap;

                try
                {
                    db.PhieuYeuCaus.Add(phieuYeuCau);

                    // Thêm chi tiết PYC và tự động tạo mặt hàng nếu chưa có
                    if (TenMH != null && TenMH.Length > 0)
                    {
                        for (int i = 0; i < TenMH.Length; i++)
                        {
                            // Lấy giá trị từ array trước để tránh lỗi LINQ
                            string tenMH_Current = TenMH[i];
                            int soLuong_Current = (SoLuong != null && i < SoLuong.Length) ? SoLuong[i] : 0;
                            
                            if (!String.IsNullOrEmpty(tenMH_Current) && soLuong_Current > 0)
                            {
                                string maMH = null;

                                // Tìm mặt hàng theo tên - lấy giá trị từ biến local thay vì array index
                                var existingMatHang = db.MatHangs.FirstOrDefault(m => m.TenMH == tenMH_Current);
                                if (existingMatHang != null)
                                {
                                    maMH = existingMatHang.MaMH;
                                }
                                else
                                {
                                    // Tạo mặt hàng mới tự động - đảm bảo unique
                                    string newMaMH = null;
                                    int nextMatHangNumber = 1;
                                    
                                    // Lấy tất cả các mã MH hiện có
                                    var existingMaMHs = db.MatHangs.Select(m => m.MaMH).ToList();
                                    
                                    // Tìm mã lớn nhất
                                    var lastMatHang = db.MatHangs.OrderByDescending(m => m.MaMH).FirstOrDefault();
                                    if (lastMatHang != null)
                                    {
                                        var lastNumber = lastMatHang.MaMH.Substring(2);
                                        if (int.TryParse(lastNumber, out int num))
                                        {
                                            nextMatHangNumber = num + 1;
                                        }
                                    }
                                    
                                    // Tìm mã chưa tồn tại
                                    while (newMaMH == null)
                                    {
                                        string candidateMaMH = "MH" + nextMatHangNumber.ToString("D3");
                                        if (!existingMaMHs.Contains(candidateMaMH))
                                        {
                                            newMaMH = candidateMaMH;
                                        }
                                        else
                                        {
                                            nextMatHangNumber++;
                                        }
                                    }
                                    
                                    maMH = newMaMH;

                                    string donViTinh_Current = (DonViTinh != null && i < DonViTinh.Length && !String.IsNullOrEmpty(DonViTinh[i])) ? DonViTinh[i] : "Cái";

                                    var newMatHang = new MatHang
                                    {
                                        MaMH = maMH,
                                        TenMH = tenMH_Current,
                                        DonViTinh = donViTinh_Current,
                                        MaLoai = null
                                    };

                                    db.MatHangs.Add(newMatHang);
                                }

                                // Xử lý nhà cung cấp: lấy từ form (TenNCC) và tìm hoặc tạo mới
                                string maNCC_Gan_Current = null;
                                string tenNCC_Current = (TenNCC != null && i < TenNCC.Length && !String.IsNullOrEmpty(TenNCC[i])) ? TenNCC[i].Trim() : null;
                                string diaChiNCC_Current = (DiaChiNCC != null && i < DiaChiNCC.Length && !String.IsNullOrEmpty(DiaChiNCC[i])) ? DiaChiNCC[i].Trim() : null;
                                string soDTNCC_Current = (SoDTNCC != null && i < SoDTNCC.Length && !String.IsNullOrEmpty(SoDTNCC[i])) ? SoDTNCC[i].Trim() : null;
                                
                                if (!String.IsNullOrEmpty(tenNCC_Current))
                                {
                                    // Tìm nhà cung cấp theo tên
                                    var existingNCC = db.NhaCungCaps.FirstOrDefault(n => n.TenNCC == tenNCC_Current);
                                    
                                    if (existingNCC != null)
                                    {
                                        // Nhà cung cấp đã tồn tại - cập nhật thông tin nếu có
                                        bool needUpdate = false;
                                        if (!String.IsNullOrEmpty(diaChiNCC_Current) && String.IsNullOrEmpty(existingNCC.DiaChi))
                                        {
                                            existingNCC.DiaChi = diaChiNCC_Current;
                                            needUpdate = true;
                                        }
                                        if (!String.IsNullOrEmpty(soDTNCC_Current) && String.IsNullOrEmpty(existingNCC.SoDT))
                                        {
                                            existingNCC.SoDT = soDTNCC_Current;
                                            needUpdate = true;
                                        }
                                        if (needUpdate)
                                        {
                                            db.Entry(existingNCC).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        
                                        maNCC_Gan_Current = existingNCC.MaNCC;
                                    }
                                    else
                                    {
                                        // Tạo nhà cung cấp mới tự động
                                        // Tạo mã nhà cung cấp tự động - đảm bảo unique
                                        string newMaNCC = null;
                                        int nextNCCNumber = 1;
                                        
                                        // Lấy tất cả các mã NCC hiện có
                                        var existingMaNCCs = db.NhaCungCaps.Select(n => n.MaNCC).ToList();
                                        
                                        // Tìm mã lớn nhất
                                        var lastNCC = db.NhaCungCaps.OrderByDescending(n => n.MaNCC).FirstOrDefault();
                                        if (lastNCC != null)
                                        {
                                            var lastNumber = lastNCC.MaNCC.Substring(3); // Bỏ "NCC" prefix
                                            if (int.TryParse(lastNumber, out int num))
                                            {
                                                nextNCCNumber = num + 1;
                                            }
                                        }
                                        
                                        // Tìm mã chưa tồn tại
                                        while (newMaNCC == null)
                                        {
                                            string candidateMaNCC = "NCC" + nextNCCNumber.ToString("D3");
                                            if (!existingMaNCCs.Contains(candidateMaNCC))
                                            {
                                                newMaNCC = candidateMaNCC;
                                            }
                                            else
                                            {
                                                nextNCCNumber++;
                                            }
                                        }
                                        
                                        // Tạo nhà cung cấp mới
                                        var newNCC = new NhaCungCap
                                        {
                                            MaNCC = newMaNCC,
                                            TenNCC = tenNCC_Current,
                                            DiaChi = diaChiNCC_Current,
                                            SoDT = soDTNCC_Current
                                        };
                                        
                                        db.NhaCungCaps.Add(newNCC);
                                        maNCC_Gan_Current = newMaNCC;
                                    }
                                }
                                
                                decimal? donGiaDuKien_Current = (DonGiaDuKien != null && i < DonGiaDuKien.Length) ? (decimal?)DonGiaDuKien[i] : (decimal?)null;

                                // Tạo chi tiết PYC
                                var chiTiet = new ChiTietPYC
                                {
                                    MaPYC = phieuYeuCau.MaPYC,
                                    MaMH = maMH,
                                    SoLuong = soLuong_Current,
                                    MaNCC_Gán = maNCC_Gan_Current,
                                    DonGiaDuKien = donGiaDuKien_Current
                                };
                                db.ChiTietPYCs.Add(chiTiet);
                            }
                        }
                    }

                    db.SaveChanges();
                    SetSuccessAlert("Phiếu yêu cầu đã được tạo thành công!", "Thành công");
                    return RedirectToAction("Index", "PhieuYeuCau");
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
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP");
                    return View();
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
                                    errorMessage += "Dữ liệu trùng lặp: Mã phiếu yêu cầu đã tồn tại trong hệ thống.";
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
                    
                    // Sử dụng ViewBag khi return View để đảm bảo alert hiển thị
                    ViewBag.AlertMessage = errorMessage;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi khi lưu dữ liệu";
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
                catch (Exception ex)
                {
                    // Sử dụng ViewBag khi return View để đảm bảo alert hiển thị
                    ViewBag.AlertMessage = "Lỗi không xác định: " + ex.Message;
                    ViewBag.AlertType = "error";
                    ViewBag.AlertTitle = "Lỗi";
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
            }

            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
            return View(phieuYeuCau);
        }

        // GET: User/PhieuYeuCauManual/GetPrice - Lấy giá mặt hàng từ database
        [HttpGet]
        public JsonResult GetPrice(string tenMH, string maNCC)
        {
            decimal? donGia = null;
            bool hasPrice = false;

            if (!string.IsNullOrEmpty(tenMH) && !string.IsNullOrEmpty(maNCC))
            {
                // Tìm mặt hàng theo tên
                var matHang = db.MatHangs.FirstOrDefault(m => m.TenMH == tenMH);
                if (matHang != null)
                {
                    // Lấy giá từ bảng giá nhà cung cấp
                    var bangGia = db.BangGiaNhaCungCaps
                        .Where(b => b.MaMH == matHang.MaMH && b.MaNCC == maNCC)
                        .OrderByDescending(b => b.NgayCapNhat)
                        .FirstOrDefault();

                    if (bangGia != null)
                    {
                        donGia = bangGia.DonGia;
                        hasPrice = true;
                    }
                }
            }

            return Json(new { donGia = donGia, hasPrice = hasPrice }, JsonRequestBehavior.AllowGet);
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

