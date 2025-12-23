using System;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Areas.User.Controllers
{
    public class TraCuuController : UserBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: User/TraCuu - Trang tra cứu chính
        public ActionResult Index()
        {
            return View();
        }

        // GET: User/TraCuu/PhieuYeuCau - Tra cứu phiếu yêu cầu
        public ActionResult PhieuYeuCau(string searchString, string trangThai, DateTime? tuNgay, DateTime? denNgay, string maBP, int page = 1)
        {
            var phieuYeuCaus = from p in db.PhieuYeuCaus
                               select p;

            // Tìm kiếm theo từ khóa
            if (!String.IsNullOrEmpty(searchString))
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.MaPYC.Contains(searchString) 
                    || p.LyDo.Contains(searchString)
                    || (p.BoPhan != null && p.BoPhan.TenBP.Contains(searchString))
                    || (p.NhanVien != null && p.NhanVien.HoTen.Contains(searchString)));
            }

            // Lọc theo trạng thái
            if (!String.IsNullOrEmpty(trangThai))
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.TrangThai == trangThai);
            }

            // Lọc theo bộ phận
            if (!String.IsNullOrEmpty(maBP))
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.MaBP_YeuCau == maBP);
            }

            // Validation: Kiểm tra năm không vượt quá 5 năm
            var today = DateTime.Now.Date;
            var minDate = today.AddYears(-5);
            var maxDate = today.AddYears(5);
            
            if (tuNgay.HasValue)
            {
                if (tuNgay.Value.Date < minDate || tuNgay.Value.Date > maxDate)
                {
                    // Nếu ngày không hợp lệ, bỏ qua filter này
                    tuNgay = null;
                }
            }
            
            if (denNgay.HasValue)
            {
                if (denNgay.Value.Date < minDate || denNgay.Value.Date > maxDate)
                {
                    // Nếu ngày không hợp lệ, bỏ qua filter này
                    denNgay = null;
                }
            }
            
            // Validation: Đến ngày >= Từ ngày
            if (tuNgay.HasValue && denNgay.HasValue && denNgay.Value.Date < tuNgay.Value.Date)
            {
                // Nếu Đến ngày < Từ ngày, bỏ qua filter Đến ngày
                denNgay = null;
            }
            
            // Lọc theo khoảng thời gian
            if (tuNgay.HasValue)
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.NgayLap.HasValue && p.NgayLap.Value >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                phieuYeuCaus = phieuYeuCaus.Where(p => p.NgayLap.HasValue && p.NgayLap.Value <= denNgay.Value.AddDays(1));
            }

            ViewBag.TrangThai = new SelectList(new[] {
                new { Value = "", Text = "Tất cả" },
                new { Value = "Chờ duyệt", Text = "Chờ duyệt" },
                new { Value = "Đã duyệt", Text = "Đã duyệt" },
                new { Value = "Từ chối", Text = "Từ chối" }
            }, "Value", "Text", trangThai);

            ViewBag.MaBP = new SelectList(db.BoPhans, "MaBP", "TenBP", maBP);
            ViewBag.SearchString = searchString;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.TrangThaiSelected = trangThai; // Giá trị string để dùng trong phân trang
            ViewBag.MaBPSelected = maBP; // Giá trị string để dùng trong phân trang

            // Phân trang: 5 items mỗi trang
            if (page < 1) page = 1;
            
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

            return View(pagedItems);
        }

        // GET: User/TraCuu/GetSuggestions - Lấy gợi ý tìm kiếm
        [HttpGet]
        public JsonResult GetSuggestions(string term)
        {
            if (String.IsNullOrEmpty(term) || term.Length < 1)
            {
                return Json(new { suggestions = new string[0] }, JsonRequestBehavior.AllowGet);
            }

            var suggestions = new System.Collections.Generic.List<string>();
            
            // Lấy các Mã PYC khớp
            var maPYCs = db.PhieuYeuCaus
                .Where(p => p.MaPYC.Contains(term))
                .Select(p => p.MaPYC)
                .Distinct()
                .Take(10)
                .ToList();
            suggestions.AddRange(maPYCs);

            // Lấy các Lý do khớp
            var lyDos = db.PhieuYeuCaus
                .Where(p => p.LyDo != null && p.LyDo.Contains(term))
                .Select(p => p.LyDo)
                .Distinct()
                .Take(5)
                .ToList();
            suggestions.AddRange(lyDos);

            // Lấy các Tên bộ phận khớp
            var boPhans = db.PhieuYeuCaus
                .Where(p => p.BoPhan != null && p.BoPhan.TenBP.Contains(term))
                .Select(p => p.BoPhan.TenBP)
                .Distinct()
                .Take(5)
                .ToList();
            suggestions.AddRange(boPhans);

            // Lấy các Tên nhân viên khớp
            var nhanViens = db.PhieuYeuCaus
                .Where(p => p.NhanVien != null && p.NhanVien.HoTen.Contains(term))
                .Select(p => p.NhanVien.HoTen)
                .Distinct()
                .Take(5)
                .ToList();
            suggestions.AddRange(nhanViens);

            // Loại bỏ trùng lặp và giới hạn số lượng
            var uniqueSuggestions = suggestions
                .Where(s => !String.IsNullOrEmpty(s))
                .Distinct()
                .Take(10)
                .ToList();

            return Json(new { suggestions = uniqueSuggestions }, JsonRequestBehavior.AllowGet);
        }

        // GET: User/TraCuu/ChiTietPYC - Tra cứu chi tiết phiếu yêu cầu
        public ActionResult ChiTietPYC(string maPYC)
        {
            if (String.IsNullOrEmpty(maPYC))
            {
                return View(new System.Collections.Generic.List<ChiTietPYC>());
            }

            var chiTiets = db.ChiTietPYCs
                .Where(c => c.MaPYC == maPYC)
                .ToList();

            ViewBag.MaPYC = maPYC;
            return View(chiTiets);
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

