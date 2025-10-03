using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly DoannewContext db;
        private readonly IMapper _mapper;

        public NhaCungCapController(DoannewContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        [Authorize(Roles = "Employee")]
        public IActionResult List()
        {
            var ds = db.NhaCungCaps.ToList();
            return View(ds);
        }

        // ================= CREATE =================
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Create(NhaCungCap model, IFormFile? Logo)
        {
            ModelState.Remove("MaNcc");
            ModelState.Remove("Logo");
            if (ModelState.IsValid)
            {
                // Gán mã NCC tự động
                model.MaNcc = GenerateMaNCC();

                // Upload logo nếu có
                if (Logo != null)
                {
                    model.Logo = MyUtil.UploadHinh(Logo, "NhaCungCap");
                }

                db.NhaCungCaps.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm nhà cung cấp thành công!";
                return RedirectToAction("List");
            }

            return View(model);
        }

        // ================= EDIT =================
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
                return NotFound();

            var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNcc == id);
            if (ncc == null)
                return NotFound();

            return View(ncc);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Edit(string id, NhaCungCap model, IFormFile? Logo)
        {
            if (id != model.MaNcc)
                return NotFound();

            ModelState.Remove("Logo");

            if (ModelState.IsValid)
            {
                var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNcc == id);
                if (ncc == null)
                    return NotFound();

                // Cập nhật dữ liệu
                ncc.TenCongTy = model.TenCongTy;
                ncc.NguoiLienLac = model.NguoiLienLac;
                ncc.Email = model.Email;
                ncc.DienThoai = model.DienThoai;
                ncc.DiaChi = model.DiaChi;
                ncc.MoTa = model.MoTa;

                // Cập nhật logo nếu có upload mới
                if (Logo != null)
                {
                    ncc.Logo = MyUtil.UploadHinh(Logo, "NhaCC");
                }

                db.Update(ncc);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
                return RedirectToAction("List");
            }

            return View(model);
        }

        // ================= DELETE =================
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (id == null)
                return NotFound();

            var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNcc == id);
            if (ncc == null)
                return NotFound();

            return View(ncc);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNcc == id);
            if (ncc == null)
                return NotFound();

            db.NhaCungCaps.Remove(ncc);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công!";
            return RedirectToAction("List");
        }

        private string GenerateMaNCC()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string ma;

            do
            {
                // Tạo ngẫu nhiên 2–4 ký tự
                ma = new string(Enumerable.Repeat(chars, 3) // 3 ký tự, bạn có thể đổi thành 2,3,4
                    .Select(s => s[random.Next(s.Length)]).ToArray());

            } while (db.NhaCungCaps.Any(n => n.MaNcc == ma)); // Đảm bảo không trùng

            return ma;
        }
    }
}
