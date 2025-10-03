using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly DoannewContext db;

        public CartController(DoannewContext context)
        {
            db = context;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        public IActionResult UpdateCart(int id, int quantity)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                item.SoLuong = quantity;
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            return View(Cart);
        }

        [HttpPost]
        public IActionResult Checkout(string hoTen, string diaChi)
        {
            var cart = Cart;
            if (cart.Count == 0)
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            if (User.Identity.IsAuthenticated && User.IsInRole("Employee"))
            {
                TempData["Message"] = "Nhân viên không được phép thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            string? maKh = null;
            if (User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                maKh = User.FindFirst("CustomerID")?.Value;
            }

            // Tạo Hóa đơn
            var hoaDon = new HoaDon
            {
                MaKh = maKh,
                NgayDat = DateTime.Now,
                HoTen = hoTen,
                DiaChi = diaChi,
                PhiVanChuyen = 0,
                MaTrangThai = 0
            };

            db.HoaDons.Add(hoaDon);
            db.SaveChanges();

            // Lưu chi tiết hóa đơn
            foreach (var item in cart)
            {
                var chiTiet = new ChiTietHd
                {
                    MaHd = hoaDon.MaHd,
                    MaHh = item.MaHh,
                    DonGia = item.DonGia,
                    SoLuong = item.SoLuong
                };
                db.ChiTietHds.Add(chiTiet);
            }

            db.SaveChanges();

            // Xóa giỏ hàng sau khi thanh toán
            HttpContext.Session.Remove(MySetting.CART_KEY);

            TempData["Message"] = "Thanh toán thành công!";
            return RedirectToAction("Invoice", new { id = hoaDon.MaHd });
        }

        public IActionResult Invoice(int id)
        {
            var hoaDon = db.HoaDons
                .Include(h => h.ChiTietHds)
                .ThenInclude(ct => ct.MaHhNavigation)
                .FirstOrDefault(h => h.MaHd == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }
    }
}