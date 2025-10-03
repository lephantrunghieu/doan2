using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.Controllers
{
    [Authorize(Roles = "Employee")]
    public class LoaiController : Controller
    {
        private readonly DoannewContext db;

        public LoaiController(DoannewContext context)
        {
            db = context;
        }

        // Danh sách loại
        public IActionResult List()
        {
            var dsLoai = db.Loais.ToList();
            return View(dsLoai);
        }

        // Thêm loại
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Loai model, IFormFile? Hinh)
        {
            if (ModelState.IsValid)
            {
                string baseAlias = GenerateAlias(model.TenLoai);
                string alias = baseAlias;
                int i = 1;

                // Kiểm tra alias đã tồn tại chưa
                while (db.HangHoas.Any(h => h.TenAlias == alias))
                {
                    alias = $"{baseAlias}-{i}";
                    i++;
                }

                model.TenLoaiAlias = alias;
                // upload hình nếu có
                if (Hinh != null)
                {
                    model.Hinh = MyUtil.UploadHinh(Hinh, "Loai");
                }

                db.Loais.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm loại hàng thành công!";
                return RedirectToAction("List");
            }
            return View(model);
        }

        // Sửa loại
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var loai = db.Loais.FirstOrDefault(l => l.MaLoai == id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        [HttpPost]
        public IActionResult Edit(int id, Loai model, IFormFile? Hinh)
        {
            if (id != model.MaLoai) return NotFound();

            if (ModelState.IsValid)
            {
                var loai = db.Loais.FirstOrDefault(l => l.MaLoai == id);
                if (loai == null) return NotFound();

                loai.TenLoai = model.TenLoai;
                string baseAlias = GenerateAlias(model.TenLoai);
                string alias = baseAlias;
                int i = 1;

                // Kiểm tra alias đã tồn tại chưa
                while (db.HangHoas.Any(h => h.TenAlias == alias))
                {
                    alias = $"{baseAlias}-{i}";
                    i++;
                }

                model.TenLoaiAlias = alias;
                loai.MoTa = model.MoTa;

                if (Hinh != null)
                {
                    loai.Hinh = MyUtil.UploadHinh(Hinh, "Loai");
                }

                db.Update(loai);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật loại hàng thành công!";
                return RedirectToAction("List");
            }

            return View(model);
        }

        // Xóa loại
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var loai = db.Loais.FirstOrDefault(l => l.MaLoai == id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var loai = db.Loais.FirstOrDefault(l => l.MaLoai == id);
            if (loai == null) return NotFound();

            db.Loais.Remove(loai);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa loại hàng thành công!";
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
