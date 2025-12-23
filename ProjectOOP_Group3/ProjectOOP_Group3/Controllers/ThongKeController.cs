using System;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Models;

namespace ProjectOOP_Group3.Controllers
{
    public class ThongKeController : Controller
    {
        private ProjectOOP_Nhom3Entities db = new ProjectOOP_Nhom3Entities();

        // GET: ThongKe - Thống kê phiếu yêu cầu theo tháng
        public ActionResult Index(int? year, int? month)
        {
            // Mặc định năm và tháng hiện tại
            if (!year.HasValue)
                year = DateTime.Now.Year;
            if (!month.HasValue)
                month = DateTime.Now.Month;

            ViewBag.Year = year;
            ViewBag.Month = month;

            // Lấy danh sách phiếu yêu cầu trong tháng
            var phieuYeuCaus = db.PhieuYeuCaus
                .Where(p => p.NgayLap.HasValue 
                    && p.NgayLap.Value.Year == year 
                    && p.NgayLap.Value.Month == month)
                .ToList();

            // Thống kê theo trạng thái
            var thongKe = new
            {
                TongSo = phieuYeuCaus.Count,
                ChoDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Chờ duyệt"),
                DaDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Đã duyệt"),
                TuChoi = phieuYeuCaus.Count(p => p.TrangThai == "Từ chối"),
                PhieuYeuCaus = phieuYeuCaus
            };

            ViewBag.ThongKe = thongKe;

            // Danh sách năm có dữ liệu
            var years = db.PhieuYeuCaus
                .Where(p => p.NgayLap.HasValue)
                .Select(p => p.NgayLap.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            ViewBag.Years = new SelectList(years, year);

            return View();
        }

        // GET: ThongKe/Chart - Biểu đồ thống kê
        public ActionResult Chart(int? year)
        {
            if (!year.HasValue)
                year = DateTime.Now.Year;

            var data = new System.Collections.Generic.List<object>();
            
            for (int m = 1; m <= 12; m++)
            {
                var count = db.PhieuYeuCaus
                    .Count(p => p.NgayLap.HasValue 
                        && p.NgayLap.Value.Year == year 
                        && p.NgayLap.Value.Month == m);
                
                data.Add(new { Month = m, Count = count });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}

