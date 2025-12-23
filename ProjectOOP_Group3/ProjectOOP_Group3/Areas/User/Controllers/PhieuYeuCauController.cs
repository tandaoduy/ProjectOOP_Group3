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
    public class PhieuYeuCauController : UserBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: User/PhieuYeuCau - Danh sách phiếu yêu cầu
        public ActionResult Index(string searchString, string trangThai, int page = 1)
        {
            var phieuYeuCaus = from p in db.PhieuYeuCaus
                               select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.MaPYC.Contains(searchString) 
                    || p.LyDo.Contains(searchString)
                    || p.BoPhan.TenBP.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(trangThai))
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.TrangThai == trangThai);
            }

            ViewBag.TrangThai = new SelectList(new[] {
                new { Value = "", Text = "Tất cả" },
                new { Value = "Chờ duyệt", Text = "Chờ duyệt" },
                new { Value = "Đã duyệt", Text = "Đã duyệt" },
                new { Value = "Từ chối", Text = "Từ chối" }
            }, "Value", "Text", trangThai);

            // Phân trang: 5 items mỗi trang
            int pageSize = 5;
            int totalItems = phieuYeuCaus.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var pagedItems = phieuYeuCaus
                .OrderByDescending(p => p.NgayLap)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchString = searchString;
            ViewBag.TrangThaiFilter = trangThai;

            return View(pagedItems);
        }

        // GET: User/PhieuYeuCau/Details/5
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

        // GET: User/PhieuYeuCau/Create
        public ActionResult Create()
        {
            // Kiểm tra xem có dữ liệu cần thiết không
            var hasMatHang = db.MatHangs.Any();
            var hasNhaCungCap = db.NhaCungCaps.Any();
            var hasBoPhan = db.BoPhans.Any();

            // Nếu thiếu dữ liệu, chuyển hướng đến trang thông báo
            if (!hasMatHang || !hasNhaCungCap || !hasBoPhan)
            {
                var missingData = new List<string>();
                if (!hasMatHang) missingData.Add("Mặt hàng");
                if (!hasNhaCungCap) missingData.Add("Nhà cung cấp");
                if (!hasBoPhan) missingData.Add("Bộ phận");
                
                TempData["MissingData"] = missingData;
                return RedirectToAction("MissingData");
            }

            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP");
            ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
            return View();
        }

        // GET: User/PhieuYeuCau/MissingData - Trang thông báo thiếu dữ liệu
        public ActionResult MissingData()
        {
            var missingData = TempData["MissingData"] as List<string> ?? new List<string>();
            ViewBag.MissingData = missingData;
            return View();
        }

        // POST: User/PhieuYeuCau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuYeuCau phieuYeuCau, string[] MaMH, int[] SoLuong, string[] MaNCC_Gan, decimal[] DonGiaDuKien)
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
                    ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
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
                    ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
                
                phieuYeuCau.MaNV_Lap = maNV_Lap;

                try
                {
                    db.PhieuYeuCaus.Add(phieuYeuCau);

                    // Thêm chi tiết PYC
                    if (MaMH != null && MaMH.Length > 0)
                    {
                        for (int i = 0; i < MaMH.Length; i++)
                        {
                            // Lấy giá trị từ array trước để tránh mọi vấn đề
                            string maMH_Current = MaMH[i];
                            int soLuong_Current = (SoLuong != null && i < SoLuong.Length) ? SoLuong[i] : 0;
                            
                            if (!String.IsNullOrEmpty(maMH_Current) && soLuong_Current > 0)
                            {
                                // Lấy các giá trị từ array trước
                                string maNCC_Gan_Current = (MaNCC_Gan != null && i < MaNCC_Gan.Length && !String.IsNullOrEmpty(MaNCC_Gan[i])) ? MaNCC_Gan[i] : null;
                                decimal? donGiaDuKien_Current = (DonGiaDuKien != null && i < DonGiaDuKien.Length) ? (decimal?)DonGiaDuKien[i] : (decimal?)null;
                                
                                var chiTiet = new ChiTietPYC
                                {
                                    MaPYC = phieuYeuCau.MaPYC,
                                    MaMH = maMH_Current,
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
                    ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
                    ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
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
                    ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
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
                    ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
                    ViewBag.MaLoai = new SelectList(db.LoaiMatHangs, "MaLoai", "TenLoai");
                    return View(phieuYeuCau);
                }
            }

            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
            ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            return View(phieuYeuCau);
        }

        // GET: User/PhieuYeuCau/GetNhaCungCapByMatHang - Lấy danh sách nhà cung cấp theo mặt hàng
        [HttpGet]
        public JsonResult GetNhaCungCapByMatHang(string maMH)
        {
            var nhaCungCaps = new List<object>();
            
            if (!string.IsNullOrEmpty(maMH))
            {
                // Lấy danh sách nhà cung cấp có trong bảng giá cho mặt hàng này
                nhaCungCaps = db.BangGiaNhaCungCaps
                    .Where(b => b.MaMH == maMH)
                    .Select(b => b.NhaCungCap)
                    .Distinct()
                    .Select(n => new { 
                        MaNCC = n.MaNCC, 
                        TenNCC = n.TenNCC 
                    })
                    .OrderBy(n => n.TenNCC)
                    .ToList<object>();
            }
            
            return Json(nhaCungCaps, JsonRequestBehavior.AllowGet);
        }

        // GET: User/PhieuYeuCau/GetPrice - Lấy giá mặt hàng từ database
        [HttpGet]
        public JsonResult GetPrice(string maMH, string maNCC)
        {
            decimal? donGia = null;
            bool hasPrice = false;
            
            // Chỉ lấy giá khi có cả mặt hàng và nhà cung cấp
            if (!string.IsNullOrEmpty(maMH) && !string.IsNullOrEmpty(maNCC))
            {
                // Lấy giá từ bảng giá nhà cung cấp
                var bangGia = db.BangGiaNhaCungCaps
                    .Where(b => b.MaMH == maMH && b.MaNCC == maNCC)
                    .OrderByDescending(b => b.NgayCapNhat)
                    .FirstOrDefault();
                
                if (bangGia != null)
                {
                    donGia = bangGia.DonGia;
                    hasPrice = true;
                }
            }
            
            return Json(new { donGia = donGia, hasPrice = hasPrice }, JsonRequestBehavior.AllowGet);
        }

        // POST: User/PhieuYeuCau/CreateMatHang - Tạo mặt hàng mới
        [HttpPost]
        public JsonResult CreateMatHang(string tenMH, string donViTinh, string maLoai)
        {
            try
            {
                // Kiểm tra mặt hàng đã tồn tại chưa
                var existingMatHang = db.MatHangs.FirstOrDefault(m => m.TenMH == tenMH);
                if (existingMatHang != null)
                {
                    return Json(new { success = true, maMH = existingMatHang.MaMH, message = "Mặt hàng đã tồn tại" });
                }

                // Tạo mã mặt hàng tự động
                var lastMatHang = db.MatHangs.OrderByDescending(m => m.MaMH).FirstOrDefault();
                int nextNumber = 1;
                if (lastMatHang != null)
                {
                    var lastNumber = lastMatHang.MaMH.Substring(2); // Bỏ "MH" prefix
                    if (int.TryParse(lastNumber, out int num))
                    {
                        nextNumber = num + 1;
                    }
                }
                string maMH = "MH" + nextNumber.ToString("D3");

                // Tạo mặt hàng mới
                var matHang = new MatHang
                {
                    MaMH = maMH,
                    TenMH = tenMH,
                    DonViTinh = donViTinh ?? "Cái",
                    MaLoai = !string.IsNullOrEmpty(maLoai) ? maLoai : null
                };

                db.MatHangs.Add(matHang);
                db.SaveChanges();

                return Json(new { success = true, maMH = maMH, message = "Đã tạo mặt hàng mới" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: User/PhieuYeuCau/SearchMatHang - Tìm kiếm mặt hàng
        [HttpGet]
        public JsonResult SearchMatHang(string term)
        {
            var matHangs = db.MatHangs
                .Where(m => m.TenMH.Contains(term))
                .Select(m => new { maMH = m.MaMH, tenMH = m.TenMH, donViTinh = m.DonViTinh })
                .Take(10)
                .ToList();

            return Json(matHangs, JsonRequestBehavior.AllowGet);
        }

        // POST: User/PhieuYeuCau/AddMatHang - Thêm mặt hàng mới từ modal
        [HttpPost]
        public JsonResult AddMatHang(string tenMH, string donViTinh, string maLoai)
        {
            try
            {
                // Kiểm tra mặt hàng đã tồn tại chưa
                var existingMatHang = db.MatHangs.FirstOrDefault(m => m.TenMH == tenMH);
                if (existingMatHang != null)
                {
                    return Json(new { success = true, maMH = existingMatHang.MaMH, tenMH = existingMatHang.TenMH, message = "Mặt hàng đã tồn tại" });
                }

                // Tạo mã mặt hàng tự động
                var lastMatHang = db.MatHangs.OrderByDescending(m => m.MaMH).FirstOrDefault();
                int nextMatHangNumber = 1;
                if (lastMatHang != null)
                {
                    var lastNumber = lastMatHang.MaMH.Substring(2); // Bỏ "MH" prefix
                    if (int.TryParse(lastNumber, out int num))
                    {
                        nextMatHangNumber = num + 1;
                    }
                }
                string maMH = "MH" + nextMatHangNumber.ToString("D3");

                // Tạo mặt hàng mới
                var matHang = new MatHang
                {
                    MaMH = maMH,
                    TenMH = tenMH,
                    DonViTinh = donViTinh ?? "Cái",
                    MaLoai = !string.IsNullOrEmpty(maLoai) ? maLoai : null
                };

                db.MatHangs.Add(matHang);
                db.SaveChanges();

                return Json(new { success = true, maMH = maMH, tenMH = tenMH, message = "Đã thêm mặt hàng mới thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: User/PhieuYeuCau/AddNhaCungCap - Thêm nhà cung cấp mới từ modal
        [HttpPost]
        public JsonResult AddNhaCungCap(string tenNCC, string diaChi, string soDT)
        {
            try
            {
                // Kiểm tra nhà cung cấp đã tồn tại chưa
                var existingNCC = db.NhaCungCaps.FirstOrDefault(n => n.TenNCC == tenNCC);
                if (existingNCC != null)
                {
                    return Json(new { success = true, maNCC = existingNCC.MaNCC, tenNCC = existingNCC.TenNCC, message = "Nhà cung cấp đã tồn tại" });
                }

                // Tạo mã nhà cung cấp tự động
                var lastNCC = db.NhaCungCaps.OrderByDescending(n => n.MaNCC).FirstOrDefault();
                int nextNCCNumber = 1;
                if (lastNCC != null)
                {
                    var lastNumber = lastNCC.MaNCC.Substring(3); // Bỏ "NCC" prefix
                    if (int.TryParse(lastNumber, out int num))
                    {
                        nextNCCNumber = num + 1;
                    }
                }
                string maNCC = "NCC" + nextNCCNumber.ToString("D3");

                // Tạo nhà cung cấp mới
                var nhaCungCap = new NhaCungCap
                {
                    MaNCC = maNCC,
                    TenNCC = tenNCC,
                    DiaChi = !string.IsNullOrEmpty(diaChi) ? diaChi : null,
                    SoDT = !string.IsNullOrEmpty(soDT) ? soDT : null
                };

                db.NhaCungCaps.Add(nhaCungCap);
                db.SaveChanges();

                return Json(new { success = true, maNCC = maNCC, tenNCC = tenNCC, message = "Đã thêm nhà cung cấp mới thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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

