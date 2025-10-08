using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly DoannewContext db;
        private readonly IMapper _mapper;

        public KhachHangController(DoannewContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        #region Register
        [HttpGet]
        public IActionResult DangKy()
        {
            TempData.Remove("SuccessMessage");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangKyAsync(RegisterVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // kiểm tra trùng mã khách hàng
                    var exists = db.KhachHangs.Any(kh => kh.MaKh == model.MaKh);
                    if (exists)
                    {
                        ModelState.AddModelError("MaKh", "Mã khách hàng đã tồn tại, vui lòng chọn mã khác.");
                        return View(model);
                    }

                    var existsEmp = db.NhanViens.Any(kh => kh.MaNv == model.MaKh);
                    if (existsEmp)
                    {
                        ModelState.AddModelError("MaKh", "Mã nhân viên đã tồn tại, vui lòng chọn mã khác.");
                        return View(model);
                    }

                    var khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = MyUtil.GenerateRamdomKey();
                    khachHang.MatKhau = GetMD5(model.MatKhau);
                    khachHang.HieuLuc = true;
                    khachHang.VaiTro = 0;

                    if (Hinh != null)
                    {
                        khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(khachHang);
                    db.SaveChanges();

                    // Lưu thông báo thành công
                    TempData["SuccessMessage"] = "Đăng ký thành công!";

                    var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, khachHang.Email),
                                new Claim(ClaimTypes.Name, khachHang.HoTen),
                                new Claim("CustomerID", khachHang.MaKh),

								//claim - role động
								new Claim(ClaimTypes.Role, "Customer")
                            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    return RedirectToAction("Index", "HangHoa");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                }
            }

            return View(model);
        }

        #endregion


        #region Login
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
                var nhanvien = db.NhanViens.SingleOrDefault(kh => kh.MaNv == model.UserName);
                if (khachHang == null && nhanvien == null)
                {
                    ModelState.AddModelError("loi", "Không có tài khoản này");
                }
                else
                {
                    if (khachHang != null && !khachHang.HieuLuc)
                    {
                        ModelState.AddModelError("loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                    }
                    else if(khachHang != null)
                    {
                        if (khachHang.MatKhau != GetMD5(model.Password))
                        {
                            ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, khachHang.Email),
                                new Claim(ClaimTypes.Name, khachHang.HoTen),
                                new Claim("CustomerID", khachHang.MaKh),

								//claim - role động
								new Claim(ClaimTypes.Role, "Customer")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                    else if (nhanvien != null)
                    {
                        if (nhanvien.MatKhau != GetMD5(model.Password))
                        {
                            ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, nhanvien.Email),
                                new Claim(ClaimTypes.Name, nhanvien.HoTen),
                                new Claim("CustomerID", nhanvien.MaNv),

								//claim - role động
								new Claim(ClaimTypes.Role, "Employee")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }
            return View();
        }
        #endregion


        public string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [Authorize(Roles = "Employee")]
        public IActionResult List()
        {
            var dsKhachHang = db.KhachHangs.ToList();
            return View(dsKhachHang);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Create(KhachHang model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                // kiểm tra trùng mã KH
                if (db.KhachHangs.Any(kh => kh.MaKh == model.MaKh))
                {
                    ModelState.AddModelError("MaKh", "Mã khách hàng đã tồn tại!");
                    return View(model);
                }

                var existsEmp = db.NhanViens.Any(kh => kh.MaNv == model.MaKh);
                if (existsEmp)
                {
                    ModelState.AddModelError("MaKh", "Mã nhân viên đã tồn tại");
                    return View(model);
                }

                // upload hình
                if (Hinh != null)
                {
                    model.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                }

                db.KhachHangs.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kh = db.KhachHangs.FirstOrDefault(k => k.MaKh == id);
            if (kh == null)
            {
                return NotFound();
            }

            return View(kh);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public IActionResult Edit(string id, KhachHang model, IFormFile? Hinh)
        {
            if (id != model.MaKh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kh = db.KhachHangs.FirstOrDefault(k => k.MaKh == id);
                    if (kh == null)
                    {
                        return NotFound();
                    }

                    // cập nhật thông tin
                    kh.HoTen = model.HoTen;
                    kh.GioiTinh = model.GioiTinh;
                    kh.NgaySinh = model.NgaySinh;
                    kh.Email = model.Email;
                    kh.DienThoai = model.DienThoai;
                    kh.DiaChi = model.DiaChi;

                    // upload lại hình nếu có
                    if (Hinh != null)
                    {
                        kh.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Update(kh);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction("List");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kh = db.KhachHangs.FirstOrDefault(x => x.MaKh == id);
            if (kh == null)
            {
                return NotFound();
            }

            return View(kh); // trả về view xác nhận xóa
        }

        // POST: KhachHang/DeleteConfirmed/maKH
        [Authorize(Roles = "Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var kh = db.KhachHangs.FirstOrDefault(x => x.MaKh == id);
            if (kh == null)
            {
                return NotFound();
            }

            db.KhachHangs.Remove(kh);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            return RedirectToAction("List");
        }

    }
}