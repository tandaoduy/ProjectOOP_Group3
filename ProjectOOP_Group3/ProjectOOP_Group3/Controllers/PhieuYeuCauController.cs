using System;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Models;

namespace ProjectOOP_Group3.Controllers
{
    public class PhieuYeuCauController : Controller
    {
        private ProjectOOP_Nhom3Entities db = new ProjectOOP_Nhom3Entities();

        // GET: PhieuYeuCau - Danh sách phiếu yêu cầu
        public ActionResult Index(string searchString, string trangThai)
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

            return View(phieuYeuCaus.ToList());
        }

        // GET: PhieuYeuCau/Details/5
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

        // GET: PhieuYeuCau/Create
        public ActionResult Create()
        {
            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP");
            ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            return View();
        }

        // POST: PhieuYeuCau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuYeuCau phieuYeuCau, string[] MaMH, int[] SoLuong, string[] MaNCC_Gan, decimal[] DonGiaDuKien)
        {
            if (ModelState.IsValid)
            {
                // Tạo mã PYC tự động
                var lastPYC = db.PhieuYeuCaus.OrderByDescending(p => p.MaPYC).FirstOrDefault();
                int nextNumber = 1;
                if (lastPYC != null)
                {
                    var lastNumber = lastPYC.MaPYC.Substring(3);
                    if (int.TryParse(lastNumber, out int num))
                    {
                        nextNumber = num + 1;
                    }
                }
                phieuYeuCau.MaPYC = "PYC" + nextNumber.ToString("D4");

                phieuYeuCau.NgayLap = DateTime.Now;
                phieuYeuCau.TrangThai = "Chờ duyệt";
                // TODO: Lấy MaNV_Lap từ session
                phieuYeuCau.MaNV_Lap = "NV001"; // Tạm thời hardcode

                db.PhieuYeuCaus.Add(phieuYeuCau);

                // Thêm chi tiết PYC
                if (MaMH != null && MaMH.Length > 0)
                {
                    for (int i = 0; i < MaMH.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(MaMH[i]) && SoLuong[i] > 0)
                        {
                            var chiTiet = new ChiTietPYC
                            {
                                MaPYC = phieuYeuCau.MaPYC,
                                MaMH = MaMH[i],
                                SoLuong = SoLuong[i],
                                MaNCC_Gán = !String.IsNullOrEmpty(MaNCC_Gan[i]) ? MaNCC_Gan[i] : null,
                                DonGiaDuKien = DonGiaDuKien != null && i < DonGiaDuKien.Length ? DonGiaDuKien[i] : null
                            };
                            db.ChiTietPYCs.Add(chiTiet);
                        }
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaBP_YeuCau = new SelectList(db.BoPhans, "MaBP", "TenBP", phieuYeuCau.MaBP_YeuCau);
            ViewBag.MaMH = new SelectList(db.MatHangs, "MaMH", "TenMH");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
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

