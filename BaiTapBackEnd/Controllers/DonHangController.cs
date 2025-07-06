using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaiTapBackEnd.Models;
using System.Security.Claims;
using System.Text.Json;

namespace BaiTapBackEnd.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public DonHangController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        // Hiển thị trang đặt hàng với danh sách giỏ hàng
        public async Task<IActionResult> DatHang()
        {
            try
            {
                var userId = GetCurrentUserId();
                List<GioHang> cartItems;
                
                if (userId.HasValue)
                {
                    // User đã đăng nhập - lấy từ database
                    cartItems = await _context.GioHangs
                        .Include(g => g.MonAn)
                        .Where(g => g.TaiKhoanId == userId)
                        .ToListAsync();
                }
                else
                {
                    // Guest user - lấy từ session
                    cartItems = GetGuestCartItems();
                }

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm món ăn vào giỏ hàng trước khi đặt hàng.";
                    return RedirectToAction("Index", "GioHang");
                }

                var totalPrice = cartItems.Sum(item => item.MonAn.Gia * item.SoLuong);
                var shippingFee = 15000m;
                var grandTotal = totalPrice + shippingFee;

                ViewBag.CartItems = cartItems;
                ViewBag.Subtotal = totalPrice;
                ViewBag.ShippingFee = shippingFee;
                ViewBag.Total = grandTotal;

                // Tạo model DonHang trống để view có thể sử dụng
                var donHang = new DonHang();
                return View(donHang);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DatHang: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang đặt hàng.";
                return RedirectToAction("Index", "GioHang");
            }
        }

        // Xử lý đặt hàng
        [HttpPost]
        public async Task<IActionResult> DatHang([Bind("TenKhachHang,SoDienThoai,DiaChi,GhiChu,PhuongThucThanhToan")] DonHang donHang)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(donHang.TenKhachHang) || string.IsNullOrWhiteSpace(donHang.SoDienThoai) || string.IsNullOrWhiteSpace(donHang.DiaChi))
                {
                    TempData["Error"] = "Vui lòng điền đầy đủ thông tin khách hàng.";
                    return RedirectToAction("DatHang");
                }

                var userId = GetCurrentUserId();
                List<GioHang> cartItems;

                if (userId.HasValue)
                {
                    // User đã đăng nhập - lấy từ database
                    cartItems = await _context.GioHangs
                        .Include(g => g.MonAn)
                        .Where(g => g.TaiKhoanId == userId)
                        .ToListAsync();
                }
                else
                {
                    // Guest user - lấy từ session
                    cartItems = GetGuestCartItems();
                }

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm món ăn vào giỏ hàng trước khi đặt hàng.";
                    return RedirectToAction("Index", "GioHang");
                }

                // Tạo đơn hàng mới
                var newDonHang = new DonHang
                {
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ xác nhận",
                    TongTien = cartItems.Sum(item => item.MonAn.Gia * item.SoLuong) + 15000m, // Thêm phí giao hàng
                    TenKhachHang = donHang.TenKhachHang,
                    SoDienThoai = donHang.SoDienThoai,
                    DiaChi = donHang.DiaChi,
                    GhiChu = donHang.GhiChu ?? "",
                    PhuongThucThanhToan = donHang.PhuongThucThanhToan ?? "Thanh toán khi nhận hàng",
                    TaiKhoanId = userId
                };

                _context.DonHangs.Add(newDonHang);
                await _context.SaveChangesAsync();

                // Tạo chi tiết đơn hàng
                foreach (var cartItem in cartItems)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        DonHangId = newDonHang.Id,
                        MonAnId = cartItem.MonAnId,
                        SoLuong = cartItem.SoLuong,
                        DonGia = cartItem.MonAn.Gia
                    };
                    _context.ChiTietDonHangs.Add(chiTietDonHang);
                }

                // Xóa toàn bộ giỏ hàng sau khi đặt hàng thành công
                if (userId.HasValue)
                {
                    var itemsToRemove = await _context.GioHangs
                        .Where(g => g.TaiKhoanId == userId)
                        .ToListAsync();
                    _context.GioHangs.RemoveRange(itemsToRemove);
                }
                else
                {
                    // Xóa khỏi session
                    ClearGuestCart();
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: {newDonHang.Id}";
                return RedirectToAction("LichSuDonHang");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;
                return RedirectToAction("DatHang");
            }
        }

        // Xem chi tiết đơn hàng
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MonAn)
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (donHang == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra quyền xem đơn hàng
                if (userId.HasValue && donHang.TaiKhoanId != userId)
                {
                    TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                    return RedirectToAction("Index", "Home");
                }

                return View(donHang);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ChiTietDonHang: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Lịch sử đơn hàng (cho user đã đăng nhập)
        public async Task<IActionResult> LichSuDonHang()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng.";
                    return RedirectToAction("DangNhap", "TaiKhoan");
                }

                var donHangs = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MonAn)
                    .Where(d => d.TaiKhoanId == userId)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                return View(donHangs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LichSuDonHang: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải lịch sử đơn hàng.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Hủy đơn hàng
        [HttpPost]
        public async Task<IActionResult> HuyDonHang(int donHangId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để thực hiện thao tác này.";
                    return RedirectToAction("DangNhap", "TaiKhoan");
                }

                var donHang = await _context.DonHangs
                    .FirstOrDefaultAsync(d => d.Id == donHangId && d.TaiKhoanId == userId);

                if (donHang == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("LichSuDonHang");
                }

                if (donHang.TrangThai != "Chờ xác nhận")
                {
                    TempData["Error"] = "Không thể hủy đơn hàng đã được xác nhận.";
                    return RedirectToAction("LichSuDonHang");
                }

                donHang.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã hủy đơn hàng thành công.";
                return RedirectToAction("LichSuDonHang");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HuyDonHang: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng.";
                return RedirectToAction("LichSuDonHang");
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

        private void RemoveSelectedFromGuestCart(List<int> selectedIds)
            {
                var cartItems = GetGuestCartItems();
            cartItems.RemoveAll(item => selectedIds.Contains(item.Id));
                SaveGuestCart(cartItems);
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

        private void ClearGuestCart()
        {
            HttpContext.Session.Remove("GuestCart");
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