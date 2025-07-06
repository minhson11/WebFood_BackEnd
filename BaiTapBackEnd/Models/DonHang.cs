using System.ComponentModel.DataAnnotations;

namespace BaiTapBackEnd.Models
{
    public class DonHang
    {
        [Key]
        public int Id { get; set; }

        public int? TaiKhoanId { get; set; } // Nullable for guest orders

        [Required(ErrorMessage = "Ngày đặt hàng là bắt buộc.")]
        public DateTime NgayDat { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
        public string TrangThai { get; set; } // e.g., Đang xử lý, Đang giao, Hoàn thành, Hủy

        [Required(ErrorMessage = "Tổng tiền là bắt buộc.")]
        [Range(0, 100000000, ErrorMessage = "Tổng tiền phải từ 0 đến 100,000,000 VNĐ.")]
        public decimal TongTien { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
        public string TenKhachHang { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
        public string DiaChi { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Phương thức thanh toán không được vượt quá 100 ký tự.")]
        public string PhuongThucThanhToan { get; set; }

        public TaiKhoan TaiKhoan { get; set; }
        public List<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}