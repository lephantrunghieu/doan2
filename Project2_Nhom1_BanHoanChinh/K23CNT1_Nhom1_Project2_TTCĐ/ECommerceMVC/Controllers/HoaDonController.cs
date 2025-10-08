using ECommerceMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly DoannewContext db;

        public HoaDonController(DoannewContext context)
        {
            db = context;
        }

        [Authorize(Roles = "Employee")]
        // Danh sách hóa đơn
        public IActionResult List()
        {
            var data = db.HoaDons
                .Include(h => h.MaKhNavigation) // khách hàng
                .Include(h => h.MaNvNavigation) // nhân viên (nếu có)
                .Include (h => h.ChiTietHds)
                .OrderByDescending(h => h.NgayDat)
                .ToList();
            return View(data);
        }

        [Authorize(Roles = "Employee")]
        // Chi tiết hóa đơn
        public IActionResult Detail(int id)
        {
            var hd = db.HoaDons
                .Include(h => h.ChiTietHds)
                    .ThenInclude(ct => ct.MaHhNavigation)
                .FirstOrDefault(h => h.MaHd == id);

            if (hd == null) return NotFound();
            return View(hd);
        }

        [Authorize(Roles = "Employee")]
        // Cập nhật trạng thái hóa đơn
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hd = db.HoaDons.Include(h => h.ChiTietHds).FirstOrDefault(h => h.MaHd == id);
            if (hd == null) return NotFound();
            return View(hd);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Edit(int id, HoaDon model)
        {
            if (id != model.MaHd) return NotFound();

            var hd = db.HoaDons.Include(h => h.ChiTietHds).FirstOrDefault(h => h.MaHd == id);
            if (hd == null) return NotFound();

            // Chỉ cho sửa trạng thái / ghi chú
            hd.MaTrangThai = model.MaTrangThai;
            hd.GhiChu = model.GhiChu;
            hd.MaNv = User.FindFirst("CustomerID")?.Value;

            db.HoaDons.Update(hd);
            db.SaveChanges();

            return RedirectToAction("List");
        }

        [Authorize]
        // Danh sách hóa đơn của khách hàng hiện tại
        public IActionResult MyOrders()
        {
            var user = User.FindFirst("CustomerID")?.Value;
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var data = db.HoaDons
                .Include(h => h.ChiTietHds)
                .Where(h => h.MaKhNavigation.MaKh == user) // liên kết với bảng khách hàng
                .OrderByDescending(h => h.NgayDat)
                .ToList();

            return View(data);
        }

        [Authorize]
        // Chi tiết hóa đơn
        public IActionResult MyOrderDetail(int id)
        {
            var user = User.FindFirst("CustomerID")?.Value;
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }

            var hoaDon = db.HoaDons
                .Include(h => h.ChiTietHds)
                .ThenInclude(ct => ct.MaHhNavigation)
                .Include(h => h.MaNvNavigation) // nhân viên (nếu có)
                .FirstOrDefault(h => h.MaHd == id && h.MaKhNavigation.MaKh == user);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }
    }
}
