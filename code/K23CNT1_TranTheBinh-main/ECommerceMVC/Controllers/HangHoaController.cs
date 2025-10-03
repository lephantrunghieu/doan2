using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly DoannewContext db;

        public HangHoaController(DoannewContext conetxt)
        {
            db = conetxt;
        }

        public IActionResult Index(int? loai, int page = 1, int pageSize = 9)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }

            int totalItems = hangHoas.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = hangHoas
        .OrderBy(p => p.MaHh)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new HangHoaVM
        {
            MaHh = p.MaHh,
            TenHH = p.TenHh,
            DonGia = p.DonGia ?? 0,
            Hinh = p.Hinh ?? "",
            MoTaNgan = p.MoTaDonVi ?? "",
            TenLoai = p.MaLoaiNavigation.TenLoai
        })
        .ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.totalItems = totalItems;
            ViewBag.CurrentLoai = loai;
            return View(result);
        }

        public IActionResult Search(string? query, int page = 1, int pageSize = 9)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }

            ViewBag.query = query;

            int totalItems = hangHoas.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = hangHoas.OrderBy(p => p.MaHh)
        .Skip((page - 1) * pageSize)
        .Take(pageSize).Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.totalItems = totalItems;
            return View(result);
        }


        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);
            if (data == null)
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            var result = new ChiTietHangHoaVM
            {
                MaHh = data.MaHh,
                TenHH = data.TenHh,
                DonGia = data.DonGia ?? 0,
                ChiTiet = data.MoTa ?? string.Empty,
                Hinh = data.Hinh ?? string.Empty,
                MoTaNgan = data.MoTaDonVi ?? string.Empty,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,//tính sau
                DiemDanhGia = 5,//check sau
            };
            var dataRelated = db.HangHoas.Where(p => p.MaHh != id).Include(p => p.MaLoaiNavigation)
    .ToList();
            ViewBag.dataRelated = dataRelated;
            data.SoLanXem += 1;
            db.HangHoas.Update(data);
            db.SaveChanges();
            return View(result);
        }

        [Authorize(Roles = "Employee")]
        public IActionResult List()
        {
            var data = db.HangHoas.ToList();
            return View(data);
        }

        // Thêm hàng hóa
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy");
            return View();
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Create(HangHoa model, IFormFile? Hinh)
        {
            ModelState.Remove("MaLoaiNavigation");
            ModelState.Remove("MaNccNavigation");
            if (ModelState.IsValid)
            {
                // Tạo alias từ tên hàng hóa
                string baseAlias = GenerateAlias(model.TenHh);
                string alias = baseAlias;
                int i = 1;

                // Kiểm tra alias đã tồn tại chưa
                while (db.HangHoas.Any(h => h.TenAlias == alias))
                {
                    alias = $"{baseAlias}-{i}";
                    i++;
                }

                model.TenAlias = alias;

                if (Hinh != null)
                {
                    model.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
                }

                model.SoLanXem = 0; // mặc định
                db.HangHoas.Add(model);
                db.SaveChanges();
                return RedirectToAction("List");
            }
            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy");
            return View(model);
        }

        // Cập nhật hàng hóa
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hh = db.HangHoas.FirstOrDefault(p => p.MaHh == id);
            if (hh == null) return NotFound();

            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy");
            return View(hh);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Edit(int id, HangHoa model, IFormFile? Hinh)
        {
            if (id != model.MaHh) return NotFound();

            ModelState.Remove("MaLoaiNavigation");
            ModelState.Remove("MaNccNavigation");
            if (ModelState.IsValid)
            {
                var hh = db.HangHoas.FirstOrDefault(p => p.MaHh == id);
                if (hh == null) return NotFound();

                hh.TenHh = model.TenHh;
                string baseAlias = GenerateAlias(model.TenHh);
                string alias = baseAlias;
                int i = 1;

                // Kiểm tra alias đã tồn tại chưa
                while (db.HangHoas.Any(h => h.TenAlias == alias))
                {
                    alias = $"{baseAlias}-{i}";
                    i++;
                }

                model.TenAlias = alias;
                hh.MaLoai = model.MaLoai;
                hh.MoTaDonVi = model.MoTaDonVi;
                hh.DonGia = model.DonGia;
                hh.NgaySx = model.NgaySx;
                hh.GiamGia = model.GiamGia;
                hh.MoTa = model.MoTa;
                hh.MaNcc = model.MaNcc;

                if (Hinh != null)
                {
                    hh.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
                }

                db.Update(hh);
                db.SaveChanges();
                return RedirectToAction("List");
            }

            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps, "MaNcc", "TenCongTy");
            return View(model);
        }

        // Xóa hàng hóa
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var hh = db.HangHoas.FirstOrDefault(p => p.MaHh == id);
            if (hh == null) return NotFound();
            return View(hh);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var hh = db.HangHoas.FirstOrDefault(p => p.MaHh == id);
            if (hh == null) return NotFound();

            db.HangHoas.Remove(hh);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        private string GenerateAlias(string ten)
        {
            return ten.ToLower().Trim()
                      .Replace(" ", "-")
                      .Replace("đ", "d")
                      .Replace("á", "a")
                      .Replace("à", "a")
                      .Replace("ã", "a")
                      .Replace("ạ", "a")
                      .Replace("ă", "a")
                      .Replace("â", "a")
                      .Replace("é", "e")
                      .Replace("è", "e")
                      .Replace("ê", "e")
                      .Replace("ó", "o")
                      .Replace("ò", "o")
                      .Replace("ô", "o")
                      .Replace("ơ", "o")
                      .Replace("ú", "u")
                      .Replace("ù", "u")
                      .Replace("ư", "u")
                      .Replace("ý", "y");
        }
    }
}