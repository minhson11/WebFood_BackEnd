using System.Diagnostics;
using BaiTapBackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaiTapBackEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShopDoAnNhanhDbcontext _context;

        public HomeController(ILogger<HomeController> logger, ShopDoAnNhanhDbcontext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tất cả món ăn
            var allMonAn = await _context.MonAns.ToListAsync();

            // Danh sách tất cả danh mục (kể cả chưa có món ăn)
            var allDanhMucs = new List<string>
            {
                "Burger", "Pizza", "Đồ uống", "Gà rán", "Mì Ý", "Combo", "Tráng miệng", "Món chay", "Món cay", "Món nướng", "Món hấp", "Món xào"
            };

            ViewBag.DanhMucs = allDanhMucs;

            return View(allMonAn); // Truyền model cho view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Test action để kiểm tra session
        public IActionResult TestSession()
        {
            var testValue = HttpContext.Session.GetString("TestKey");
            if (string.IsNullOrEmpty(testValue))
            {
                HttpContext.Session.SetString("TestKey", "Session is working!");
                ViewBag.Message = "Session đã được tạo thành công!";
            }
            else
            {
                ViewBag.Message = $"Session value: {testValue}";
            }
            return View();
        }
    }
}
