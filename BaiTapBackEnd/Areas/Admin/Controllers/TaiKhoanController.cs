using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaiTapBackEnd.Models;

namespace BaiTapBackEnd.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public TaiKhoanController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        public IActionResult DanhSach()
        {
            var dsTaiKhoan = _context.TaiKhoans.ToList();
            return View(dsTaiKhoan);
        }

        public IActionResult GanQuyen(int id)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.Id == id);
            if (taiKhoan == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("DanhSach");
            }
            return View(taiKhoan);
        }

        [HttpPost]
        public IActionResult GanQuyen(int id, string vaiTro)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.Id == id);
            if (taiKhoan == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Tài khoản không tồn tại." });

                TempData["ThongBao"] = "Tài khoản không tồn tại.";
                return RedirectToAction("DanhSach");
            }

            if (string.IsNullOrEmpty(vaiTro) || (vaiTro != "Admin" && vaiTro != "User"))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Vai trò không hợp lệ." });

                TempData["ThongBao"] = "Vai trò không hợp lệ.";
                return RedirectToAction("DanhSach");
            }

            taiKhoan.VaiTro = vaiTro;
            _context.SaveChanges();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = $"Đã gán quyền {vaiTro} cho tài khoản." });

            TempData["ThongBao"] = "Gán quyền thành công.";
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public IActionResult XoaTaiKhoan(int id)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.Id == id);
            if (taiKhoan == null)
            {
                TempData["ThongBao"] = "Tài khoản không tồn tại.";
                return RedirectToAction("DanhSach");
            }

            _context.TaiKhoans.Remove(taiKhoan);
            _context.SaveChanges();

            TempData["ThongBao"] = "Xóa tài khoản thành công.";
            return RedirectToAction("DanhSach");
        }
    }

}

