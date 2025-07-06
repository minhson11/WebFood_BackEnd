using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaiTapBackEnd.Models;
using System.Security.Claims;

namespace BaiTapBackEnd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public DonHangController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        // Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> Index(string trangThai = "", string searchTerm = "")
        {
            try
            {
                var query = _context.DonHangs
                    .Include(d => d.TaiKhoan)
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MonAn)
                    .AsQueryable();

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai))
                {
                    query = query.Where(d => d.TrangThai == trangThai);
                }

                // Tìm kiếm theo tên khách hàng, số điện thoại hoặc mã đơn hàng
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(d => 
                        d.TenKhachHang.Contains(searchTerm) || 
                        d.SoDienThoai.Contains(searchTerm) || 
                        d.Id.ToString().Contains(searchTerm));
                }

                var donHangs = await query
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                ViewBag.TrangThai = trangThai;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.TrangThaiList = new List<string> 
                { 
                    "Chờ xác nhận", 
                    "Đã xác nhận", 
                    "Đang giao", 
                    "Hoàn thành", 
                    "Đã hủy" 
                };

                return View(donHangs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Admin DonHang Index: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách đơn hàng.";
                return View(new List<DonHang>());
            }
        }

        // Xem chi tiết đơn hàng
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MonAn)
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (donHang == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Index");
                }

                ViewBag.TrangThaiList = new List<string> 
                { 
                    "Chờ xác nhận", 
                    "Đã xác nhận", 
                    "Đang giao", 
                    "Hoàn thành", 
                    "Đã hủy" 
                };

                return View(donHang);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Admin DonHang ChiTiet: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng.";
                return RedirectToAction("Index");
            }
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int donHangId, string trangThaiMoi)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(donHangId);
                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Kiểm tra tính hợp lệ của trạng thái
                var trangThaiHopLe = new List<string> 
                { 
                    "Chờ xác nhận", 
                    "Đã xác nhận", 
                    "Đang giao", 
                    "Hoàn thành", 
                    "Đã hủy" 
                };

                if (!trangThaiHopLe.Contains(trangThaiMoi))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ." });
                }

                // Kiểm tra logic chuyển đổi trạng thái
                if (donHang.TrangThai == "Đã hủy" && trangThaiMoi != "Đã hủy")
                {
                    return Json(new { success = false, message = "Không thể thay đổi trạng thái của đơn hàng đã hủy." });
                }

                if (donHang.TrangThai == "Hoàn thành" && trangThaiMoi != "Hoàn thành")
                {
                    return Json(new { success = false, message = "Không thể thay đổi trạng thái của đơn hàng đã hoàn thành." });
                }

                donHang.TrangThai = trangThaiMoi;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CapNhatTrangThai: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái." });
            }
        }
    }
} 