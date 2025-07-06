using BaiTapBackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace BaiTapBackEnd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MonAnController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;
        private readonly IWebHostEnvironment _env;

        public MonAnController(ShopDoAnNhanhDbcontext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private void SetDanhMucList(string? selected = null)
        {
            var danhMucArr = new[]
            {
                "Burger", "Pizza", "Đồ uống", "Gà rán", "Mì Ý", "Combo", "Khác"
            };
            // Nếu selected không nằm trong danh sách thì truyền null
            var selectedValue = danhMucArr.Contains(selected) ? selected : null;
            ViewBag.DanhMucList = new SelectList(danhMucArr, selectedValue);
        }

        public IActionResult Index()
        {
            return View(_context.MonAns.ToList());
        }

        public IActionResult Them()
        {
            SetDanhMucList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Them(MonAn monAn, IFormFile? fileHinhAnh)
        {
            SetDanhMucList();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return View(monAn);
            }

            if (fileHinhAnh != null && fileHinhAnh.Length > 0)
            {
                var ext = Path.GetExtension(fileHinhAnh.FileName).ToLower();
                if (!new[] { ".jpg", ".png", ".jpeg", ".gif" }.Contains(ext))
                {
                    ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận ảnh định dạng .jpg, .png, .jpeg, .gif.");
                    return View(monAn);
                }

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    fileHinhAnh.CopyTo(stream);
                }

                monAn.HinhAnh = fileName;
            }

            _context.MonAns.Add(monAn);
            _context.SaveChanges();
            TempData["ThongBao"] = "Thêm món ăn thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Sua(int id)
        {
            var monAn = _context.MonAns.Find(id);
            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn!";
                return RedirectToAction("Index");
            }
            // Luôn set danh mục, không cần kiểm tra selected
            ViewBag.DanhMucList = new SelectList(new[]
            {
                "Burger", "Pizza", "Đồ uống", "Gà rán", "Mì Ý", "Combo", "Khác"
            }, monAn.DanhMuc);
            return View(monAn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sua(int id, MonAn monAn, IFormFile? fileHinhAnh)
        {
            var monAnCu = _context.MonAns.Find(id);
            if (monAnCu == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn!";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                return View(monAn);
            }

            // Cập nhật thông tin cơ bản
            monAnCu.Ten = monAn.Ten;
            monAnCu.MoTa = monAn.MoTa;
            monAnCu.Gia = monAn.Gia;
            monAnCu.DanhMuc = monAn.DanhMuc;

            // Xử lý ảnh mới nếu có
            if (fileHinhAnh != null && fileHinhAnh.Length > 0)
            {
                var ext = Path.GetExtension(fileHinhAnh.FileName).ToLower();
                if (!new[] { ".jpg", ".png", ".jpeg", ".gif" }.Contains(ext))
                {
                    ModelState.AddModelError("HinhAnh", "Chỉ chấp nhận ảnh định dạng .jpg, .png, .jpeg, .gif.");
                    return View(monAn);
                }
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(monAnCu.HinhAnh))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, "images", monAnCu.HinhAnh);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    fileHinhAnh.CopyTo(stream);
                }
                monAnCu.HinhAnh = fileName;
            }

            _context.SaveChanges();
            TempData["ThongBao"] = "Cập nhật món ăn thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult ChiTiet(int id)
        {
            var monAn = _context.MonAns.Find(id);
            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn!";
                return RedirectToAction("Index");
            }
            return View(monAn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Xoa(int id)
        {
            var monAn = _context.MonAns.Find(id);
            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn!";
                return RedirectToAction("Index");
            }
            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(monAn.HinhAnh))
            {
                var imagePath = Path.Combine(_env.WebRootPath, "images", monAn.HinhAnh);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _context.MonAns.Remove(monAn);
            _context.SaveChanges();
            TempData["ThongBao"] = "Xóa món ăn thành công!";
            return RedirectToAction("Index");
        }
    }
}
