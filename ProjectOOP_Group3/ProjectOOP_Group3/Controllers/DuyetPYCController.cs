using System;
using System.Linq;
using System.Web.Mvc;
using ProjectOOP_Group3.Models;

namespace ProjectOOP_Group3.Controllers
{
    public class DuyetPYCController : Controller
    {
        private ProjectOOP_Nhom3Entities db = new ProjectOOP_Nhom3Entities();

        // GET: DuyetPYC - Danh sách phiếu yêu cầu chờ duyệt
        public ActionResult Index()
        {
            var phieuYeuCaus = db.PhieuYeuCaus
                .Where(p => p.TrangThai == "Chờ duyệt")
                .OrderBy(p => p.NgayLap)
                .ToList();
            return View(phieuYeuCaus);
        }

        // GET: DuyetPYC/Details/5 - Xem chi tiết phiếu yêu cầu
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

        // GET: DuyetPYC/Duyet/5 - Form duyệt phiếu
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

        // POST: DuyetPYC/Duyet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Duyet(string id, string trangThai, string ghiChuDuyet)
        {
            PhieuYeuCau phieuYeuCau = db.PhieuYeuCaus.Find(id);
            if (phieuYeuCau == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                phieuYeuCau.TrangThai = trangThai;
                phieuYeuCau.NgayDuyet = DateTime.Now;
                phieuYeuCau.GhiChuDuyet = ghiChuDuyet;
                // TODO: Lấy MaNV_Duyet từ session
                phieuYeuCau.MaNV_Duyet = "NV002"; // Tạm thời hardcode (Kế toán trưởng)

                db.SaveChanges();
                return RedirectToAction("Index");
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

