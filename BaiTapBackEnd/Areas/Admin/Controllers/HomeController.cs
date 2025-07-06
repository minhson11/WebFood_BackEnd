using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaiTapBackEnd.Models;

namespace BaiTapBackEnd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public HomeController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            
            return View();
        }
    }
} 