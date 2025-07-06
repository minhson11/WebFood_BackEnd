using Microsoft.EntityFrameworkCore;

namespace BaiTapBackEnd.Models
{
    public class ShopDoAnNhanhDbcontext : DbContext
    {
        public ShopDoAnNhanhDbcontext(DbContextOptions<ShopDoAnNhanhDbcontext> options) : base(options)
        {
        }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<MonAn> MonAns { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
