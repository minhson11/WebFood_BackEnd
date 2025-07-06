using System.ComponentModel.DataAnnotations;

namespace BaiTapBackEnd.Models
{
    public class GioHang
    {
        [Key]
        public int Id { get; set; }

        public int? TaiKhoanId { get; set; } // Nullable for guest carts

        [Required(ErrorMessage = "Món ăn là bắt buộc.")]
        public int MonAnId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")]
        public int SoLuong { get; set; }

        public TaiKhoan TaiKhoan { get; set; }
        public MonAn MonAn { get; set; }
    }
}