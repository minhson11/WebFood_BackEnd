using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaiTapBackEnd.Models;
using System.Security.Claims;
using System.Text.Json;

namespace BaiTapBackEnd.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public GioHangController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                List<GioHang> cartItems;
                
                if (userId.HasValue)
                {
                    // User đã đăng nhập - lấy từ database
                    cartItems = await GetCartItems(userId);
                }
                else
                {
                    // Guest user - lấy từ session
                    cartItems = GetGuestCartItems();
                }
                
                ViewBag.TotalPrice = cartItems?.Sum(item => item.MonAn?.Gia * item.SoLuong) ?? 0;
                return View(cartItems ?? new List<GioHang>());
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error in Index: {ex.Message}");
                return View(new List<GioHang>());
            }
        }

        // Thêm món ăn vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemVaoGioHang(int monAnId, int soLuong = 1)
        {
            try
            {
                if (soLuong <= 0 || soLuong > 100)
                {
                    TempData["Error"] = "Số lượng không hợp lệ.";
                    return RedirectToAction("ChiTiet", "MonAn", new { id = monAnId });
                }

                var userId = GetCurrentUserId();
                var monAn = await _context.MonAns.FindAsync(monAnId);
                if (monAn == null)
                {
                    TempData["Error"] = "Món ăn không tồn tại.";
                    return RedirectToAction("Index", "MonAn");
                }

                if (userId.HasValue)
                {
                    var existingItem = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.MonAnId == monAnId && g.TaiKhoanId == userId);

                    if (existingItem != null)
                    {
                        existingItem.SoLuong += soLuong;
                        if (existingItem.SoLuong > 100)
                        {
                            existingItem.SoLuong = 100;
                        }
                    }
                    else
                    {
                        var gioHangItem = new GioHang
                        {
                            MonAnId = monAnId,
                            SoLuong = soLuong,
                            TaiKhoanId = userId
                        };
                        _context.GioHangs.Add(gioHangItem);
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    AddToGuestCart(monAnId, soLuong);
                }

                TempData["Success"] = $"Đã thêm {soLuong} {monAn.Ten} vào giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm vào giỏ hàng.";
                return RedirectToAction("ChiTiet", "MonAn", new { id = monAnId });
            }
        }

        // Cập nhật số lượng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatSoLuong(int gioHangId, int soLuong)
        {
            try
            {
                if (soLuong < 1 || soLuong > 100)
                {
                    TempData["Error"] = "Số lượng không hợp lệ.";
                    return RedirectToAction("Index");
                }

                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    var gioHangItem = await _context.GioHangs
                        .Include(g => g.MonAn)
                        .FirstOrDefaultAsync(g => g.Id == gioHangId && g.TaiKhoanId == userId);

                    if (gioHangItem == null)
                    {
                        TempData["Error"] = "Không tìm thấy item trong giỏ hàng.";
                        return RedirectToAction("Index");
                    }

                    gioHangItem.SoLuong = soLuong;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    UpdateGuestCartItem(gioHangId, soLuong);
                }

                TempData["Success"] = "Cập nhật số lượng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật giỏ hàng.";
                return RedirectToAction("Index");
            }
        }

        // Xóa item khỏi giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhoiGioHang(int gioHangId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    var gioHangItem = await _context.GioHangs
                        .FirstOrDefaultAsync(g => g.Id == gioHangId && g.TaiKhoanId == userId);

                    if (gioHangItem == null)
                    {
                        TempData["Error"] = "Không tìm thấy item trong giỏ hàng.";
                        return RedirectToAction("Index");
                    }

                    _context.GioHangs.Remove(gioHangItem);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    RemoveFromGuestCart(gioHangId);
                }

                TempData["Success"] = "Đã xóa món khỏi giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa món ăn.";
                return RedirectToAction("Index");
            }
        }

        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public async Task<IActionResult> XoaToanBoGioHang()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                if (userId.HasValue)
                {
                    // User đã đăng nhập - xóa từ database
                    var cartItems = await _context.GioHangs
                        .Where(g => g.TaiKhoanId == userId)
                        .ToListAsync();

                    _context.GioHangs.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Guest user - xóa từ session
                    HttpContext.Session.Remove("GuestCart");
                }

                return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in XoaToanBoGioHang: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa giỏ hàng." });
            }
        }

        // Lấy số lượng item trong giỏ hàng (cho navbar)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartCount = await GetCartCount(userId);
                return Json(new { cartCount = cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCartCount: {ex.Message}");
                return Json(new { cartCount = 0 });
            }
        }

        private async Task<List<GioHang>> GetCartItems(int? userId)
        {
            try
            {
                return await _context.GioHangs
                    .Include(g => g.MonAn)
                    .Where(g => g.TaiKhoanId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCartItems: {ex.Message}");
                return new List<GioHang>();
            }
        }

        private List<GioHang> GetGuestCartItems()
        {
            try
            {
                var cartJson = HttpContext.Session.GetString("GuestCart");
                if (string.IsNullOrEmpty(cartJson))
                    return new List<GioHang>();

                var cartData = JsonSerializer.Deserialize<List<GioHang>>(cartJson);
                if (cartData == null)
                    return new List<GioHang>();

                var cartItems = new List<GioHang>();
                
                foreach (var item in cartData)
                {
                    var monAn = _context.MonAns.Find(item.MonAnId);
                    if (monAn != null)
                    {
                        cartItems.Add(new GioHang
                        {
                            Id = item.Id,
                            MonAnId = item.MonAnId,
                            SoLuong = item.SoLuong,
                            MonAn = monAn
                        });
                    }
                }
                
                return cartItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGuestCartItems: {ex.Message}");
                return new List<GioHang>();
            }
        }

        private void AddToGuestCart(int monAnId, int soLuong)
        {
            try
            {
                var cartItems = GetGuestCartItems();
                var existingItem = cartItems.FirstOrDefault(i => i.MonAnId == monAnId);
                
                if (existingItem != null)
                {
                    existingItem.SoLuong += soLuong;
                    if (existingItem.SoLuong > 100)
                    {
                        existingItem.SoLuong = 100;
                    }
                }
                else
                {
                    var newId = cartItems.Count > 0 ? cartItems.Max(i => i.Id) + 1 : 1;
                    cartItems.Add(new GioHang
                    {
                        Id = newId,
                        MonAnId = monAnId,
                        SoLuong = soLuong
                    });
                }
                
                SaveGuestCart(cartItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddToGuestCart: {ex.Message}");
            }
        }

        private void UpdateGuestCartItem(int itemId, int soLuong)
        {
            try
            {
                var cartItems = GetGuestCartItems();
                var item = cartItems.FirstOrDefault(i => i.Id == itemId);
                
                if (item != null)
                {
                    if (soLuong <= 0)
                    {
                        cartItems.Remove(item);
                    }
                    else
                    {
                        item.SoLuong = soLuong;
                    }
                    SaveGuestCart(cartItems);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateGuestCartItem: {ex.Message}");
            }
        }

        private void RemoveFromGuestCart(int itemId)
        {
            try
            {
                var cartItems = GetGuestCartItems();
                var item = cartItems.FirstOrDefault(i => i.Id == itemId);
                
                if (item != null)
                {
                    cartItems.Remove(item);
                    SaveGuestCart(cartItems);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFromGuestCart: {ex.Message}");
            }
        }

        private void SaveGuestCart(List<GioHang> cartItems)
        {
            try
            {
                // Chỉ lưu các thuộc tính cần thiết để tránh circular reference
                var guestItems = cartItems.Select(i => new
                {
                    Id = i.Id,
                    MonAnId = i.MonAnId,
                    SoLuong = i.SoLuong
                }).ToList();
                
                var cartJson = JsonSerializer.Serialize(guestItems);
                HttpContext.Session.SetString("GuestCart", cartJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveGuestCart: {ex.Message}");
            }
        }

        private async Task<int> GetCartCount(int? userId)
        {
            try
            {
                if (userId.HasValue)
                {
                    return await _context.GioHangs
                        .Where(g => g.TaiKhoanId == userId)
                        .SumAsync(g => g.SoLuong);
                }
                else
                {
                    var guestItems = GetGuestCartItems();
                    return guestItems?.Sum(i => i.SoLuong) ?? 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCartCount: {ex.Message}");
                return 0;
            }
        }

        private int? GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentUserId: {ex.Message}");
                return null;
            }
        }
    }
} 