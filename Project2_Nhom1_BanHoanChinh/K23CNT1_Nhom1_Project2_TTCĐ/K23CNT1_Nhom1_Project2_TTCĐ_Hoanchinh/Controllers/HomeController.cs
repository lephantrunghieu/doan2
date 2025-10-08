using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing.Printing;

namespace ECommerceMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DoannewContext db;

        public HomeController(ILogger<HomeController> logger, DoannewContext conetxt)
        {
            _logger = logger;
            db = conetxt;
        }

        public IActionResult Index()
        {

            int page = 1;
            int pageSize = 9;
            var hangHoas = db.HangHoas.AsQueryable();

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
            return View(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}