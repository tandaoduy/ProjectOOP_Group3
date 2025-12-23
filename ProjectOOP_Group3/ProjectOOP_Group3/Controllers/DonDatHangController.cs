using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Models;

namespace ProjectOOP_Group3.Controllers
{
    public class DonDatHangController : Controller
    {
        private ProjectOOP_Nhom3Entities db = new ProjectOOP_Nhom3Entities();

        // GET: DonDatHang - Danh sách đơn đặt hàng
        public ActionResult Index()
        {
            var donDatHangs = db.DonDatHangs.Include("PhieuYeuCau").Include("NhaCungCap").ToList();
            return View(donDatHangs);
        }

        // GET: DonDatHang/Create - Tạo đơn đặt hàng từ phiếu yêu cầu đã duyệt
        public ActionResult Create(string maPYC)
        {
            if (String.IsNullOrEmpty(maPYC))
            {
                // Hiển thị danh sách phiếu yêu cầu đã duyệt để chọn
                var phieuYeuCaus = db.PhieuYeuCaus
                    .Where(p => p.TrangThai == "Đã duyệt" && !db.DonDatHangs.Any(d => d.MaPYC == p.MaPYC))
                    .ToList();
                ViewBag.PhieuYeuCaus = phieuYeuCaus;
                return View("ChonPhieuYeuCau");
            }

            var phieuYeuCau = db.PhieuYeuCaus.Find(maPYC);
            if (phieuYeuCau == null || phieuYeuCau.TrangThai != "Đã duyệt")
            {
                return HttpNotFound();
            }

            ViewBag.MaPYC = maPYC;
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC");
            ViewBag.ChiTietPYC = phieuYeuCau.ChiTietPYCs.ToList();
            return View();
        }

        // POST: DonDatHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DonDatHang donDatHang)
        {
            if (ModelState.IsValid)
            {
                // Tạo mã DDH tự động
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
                donDatHang.MaDDH = "DDH" + nextNumber.ToString("D4");

                donDatHang.NgayLap = DateTime.Now;

                // Tính tổng tiền từ chi tiết PYC
                var phieuYeuCau = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                if (phieuYeuCau != null)
                {
                    decimal tongTien = 0;
                    foreach (var chiTiet in phieuYeuCau.ChiTietPYCs)
                    {
                        if (chiTiet.DonGiaDuKien.HasValue)
                        {
                            tongTien += chiTiet.DonGiaDuKien.Value * chiTiet.SoLuong;
                        }
                    }
                    donDatHang.TongTien = tongTien;
                }

                db.DonDatHangs.Add(donDatHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaNCC = new SelectList(db.NhaCungCaps, "MaNCC", "TenNCC", donDatHang.MaNCC);
            if (!String.IsNullOrEmpty(donDatHang.MaPYC))
            {
                var phieuYeuCau = db.PhieuYeuCaus.Find(donDatHang.MaPYC);
                ViewBag.ChiTietPYC = phieuYeuCau?.ChiTietPYCs.ToList();
            }
            return View(donDatHang);
        }

        // GET: DonDatHang/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            DonDatHang donDatHang = db.DonDatHangs.Find(id);
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

