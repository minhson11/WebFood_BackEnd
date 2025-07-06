using System.ComponentModel.DataAnnotations;

namespace BaiTapBackEnd.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Đơn hàng là bắt buộc.")]
        public int DonHangId { get; set; }

        [Required(ErrorMessage = "Món ăn là bắt buộc.")]
        public int MonAnId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
        [Range(0, 10000000, ErrorMessage = "Đơn giá phải từ 0 đến 10,000,000 VNĐ.")]
        public decimal DonGia { get; set; }

        public DonHang DonHang { get; set; }
        public MonAn MonAn { get; set; }
    }
}