using DoAn2.Data;
using DoAn2.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DoAn2.ViewComponents

{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly DoannewContext db;

        public MenuLoaiViewComponent(DoannewContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
               MaLoai= lo.MaLoai,
               TenLoai = lo.TenLoai,
               SoLuong = lo.HangHoas.Count
            }).OrderBy(p => p.TenLoai);
            return View(data);
        }
    }
}
