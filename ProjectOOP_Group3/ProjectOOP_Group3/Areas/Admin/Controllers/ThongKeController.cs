using System;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Collections.Generic;
using ProjectOOP_Group3.Modal;
using ProjectOOP_Group3.Areas.Admin.Models;
using ProjectOOP_Group3.Helpers;

namespace ProjectOOP_Group3.Areas.Admin.Controllers
{
    public class ThongKeController : AdminBaseController
    {
        private ProjectOOP_Nhom3Entities1 db = new ProjectOOP_Nhom3Entities1();

        // GET: Admin/ThongKe - Thống kê phiếu yêu cầu theo tháng hoặc ngày
        public ActionResult Index(int? year, int? month, DateTime? selectedDate, string loaiThongKe = "thang")
        {
            ViewBag.LoaiThongKe = loaiThongKe;

            List<PhieuYeuCau> phieuYeuCaus = new List<PhieuYeuCau>();
            ThongKeViewModel thongKe = null;

            if (loaiThongKe == "ngay")
            {
                // Thống kê theo ngày
                if (!selectedDate.HasValue)
                    selectedDate = DateTime.Now.Date;

                ViewBag.SelectedDate = selectedDate.Value.ToString("yyyy-MM-dd");

                // Lấy danh sách phiếu yêu cầu trong ngày
                // Entity Framework không hỗ trợ .Date, nên so sánh Year, Month, Day riêng biệt
                var selectedYear = selectedDate.Value.Year;
                var selectedMonth = selectedDate.Value.Month;
                var selectedDay = selectedDate.Value.Day;
                
                phieuYeuCaus = db.PhieuYeuCaus
                    .Where(p => p.NgayLap.HasValue 
                        && p.NgayLap.Value.Year == selectedYear
                        && p.NgayLap.Value.Month == selectedMonth
                        && p.NgayLap.Value.Day == selectedDay)
                    .ToList();

                // Thống kê theo trạng thái
                thongKe = new ThongKeViewModel
                {
                    TongSo = phieuYeuCaus.Count,
                    ChoDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Chờ duyệt"),
                    DaDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Đã duyệt"),
                    TuChoi = phieuYeuCaus.Count(p => p.TrangThai == "Từ chối"),
                    PhieuYeuCaus = phieuYeuCaus
                };
            }
            else
            {
                // Thống kê theo tháng (mặc định)
                if (!year.HasValue)
                    year = DateTime.Now.Year;
                if (!month.HasValue)
                    month = DateTime.Now.Month;

                ViewBag.Year = year;
                ViewBag.Month = month;

                // Lấy danh sách phiếu yêu cầu trong tháng
                phieuYeuCaus = db.PhieuYeuCaus
                    .Where(p => p.NgayLap.HasValue 
                        && p.NgayLap.Value.Year == year 
                        && p.NgayLap.Value.Month == month)
                    .ToList();

                // Thống kê theo trạng thái
                thongKe = new ThongKeViewModel
                {
                    TongSo = phieuYeuCaus.Count,
                    ChoDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Chờ duyệt"),
                    DaDuyet = phieuYeuCaus.Count(p => p.TrangThai == "Đã duyệt"),
                    TuChoi = phieuYeuCaus.Count(p => p.TrangThai == "Từ chối"),
                    PhieuYeuCaus = phieuYeuCaus
                };
            }

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

        // GET: Admin/ThongKe/Chart - Biểu đồ thống kê
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

        // GET: Admin/ThongKe/ExportExcel - Export thống kê ra Excel
        [HttpGet]
        public ActionResult ExportExcel(int? year, int? month, DateTime? selectedDate, string loaiThongKe = "thang")
        {
            List<PhieuYeuCau> phieuYeuCaus = new List<PhieuYeuCau>();
            string title = "";
            string fileName = "";

            if (loaiThongKe == "ngay")
            {
                // Thống kê theo ngày
                if (!selectedDate.HasValue)
                    selectedDate = DateTime.Now.Date;

                // Lấy danh sách phiếu yêu cầu trong ngày
                // Entity Framework không hỗ trợ .Date, nên so sánh Year, Month, Day riêng biệt
                var selectedYear = selectedDate.Value.Year;
                var selectedMonth = selectedDate.Value.Month;
                var selectedDay = selectedDate.Value.Day;
                
                phieuYeuCaus = db.PhieuYeuCaus
                    .Where(p => p.NgayLap.HasValue 
                        && p.NgayLap.Value.Year == selectedYear
                        && p.NgayLap.Value.Month == selectedMonth
                        && p.NgayLap.Value.Day == selectedDay)
                    .OrderBy(p => p.NgayLap)
                    .ToList();

                title = $"BÁO CÁO THỐNG KÊ PHIẾU YÊU CẦU MUA HÀNG - Ngày {selectedDate.Value:dd/MM/yyyy}";
                fileName = $"ThongKe_Ngay{selectedDate.Value:yyyyMMdd}_{DateTime.Now:yyyyMMddHHmmss}.xls";
            }
            else
            {
                // Thống kê theo tháng
                if (!year.HasValue)
                    year = DateTime.Now.Year;
                if (!month.HasValue)
                    month = DateTime.Now.Month;

                // Lấy danh sách phiếu yêu cầu trong tháng
                phieuYeuCaus = db.PhieuYeuCaus
                    .Where(p => p.NgayLap.HasValue 
                        && p.NgayLap.Value.Year == year 
                        && p.NgayLap.Value.Month == month)
                    .OrderBy(p => p.NgayLap)
                    .ToList();

                title = $"BÁO CÁO THỐNG KÊ PHIẾU YÊU CẦU MUA HÀNG - Tháng {month}/{year}";
                fileName = $"ThongKe_Thang{month}_{year}_{DateTime.Now:yyyyMMddHHmmss}.xls";
            }

            // Tạo dữ liệu cho Excel
            var excelData = new List<Dictionary<string, object>>();
            
            // Thêm header cho bảng chi tiết (chỉ 2 cột: Tên và Giá trị)
            excelData.Add(new Dictionary<string, object> { { "Cột1", "THỐNG KÊ" }, { "Cột2", "GIÁ TRỊ" } });
            
            // Thêm chi tiết
            foreach (var item in phieuYeuCaus)
            {
                excelData.Add(new Dictionary<string, object>
                {
                    { "Cột1", item.MaPYC },
                    { "Cột2", item.NgayLap?.ToString("dd/MM/yyyy") ?? "" }
                });
            }
            
            // Thêm dòng trống
            excelData.Add(new Dictionary<string, object> { { "Cột1", "" }, { "Cột2", "" } });
            
            // Thêm thống kê tổng quan xuống dưới cùng (chỉ 2 cột)
            excelData.Add(new Dictionary<string, object> { { "Cột1", "THỐNG KÊ TỔNG QUAN" }, { "Cột2", "" } });
            excelData.Add(new Dictionary<string, object> { { "Cột1", "Tổng số" }, { "Cột2", phieuYeuCaus.Count } });
            excelData.Add(new Dictionary<string, object> { { "Cột1", "Chờ duyệt" }, { "Cột2", phieuYeuCaus.Count(p => p.TrangThai == "Chờ duyệt") } });
            excelData.Add(new Dictionary<string, object> { { "Cột1", "Đã duyệt" }, { "Cột2", phieuYeuCaus.Count(p => p.TrangThai == "Đã duyệt") } });
            excelData.Add(new Dictionary<string, object> { { "Cột1", "Từ chối" }, { "Cột2", phieuYeuCaus.Count(p => p.TrangThai == "Từ chối") } });

            // Tạo file Excel
            var fileBytes = ExcelHelper.CreateExcelFile(excelData, title);
            
            return File(fileBytes, "application/vnd.ms-excel", fileName);
        }
    }
}

