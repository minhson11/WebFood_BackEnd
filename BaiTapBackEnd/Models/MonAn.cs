using System.ComponentModel.DataAnnotations;

namespace BaiTapBackEnd.Models
{
    public class MonAn
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống.")]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        public string? MoTa { get; set; }

        [Range(0, 10000000, ErrorMessage = "Giá phải từ 0 đến 10 triệu.")]
        public decimal Gia { get; set; }

        public string? HinhAnh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public string? DanhMuc { get; set; }
    }
}
