using BaiTapBackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaiTapBackEnd.Controllers
{
    public class MonAnController : Controller
    {
        private readonly ShopDoAnNhanhDbcontext _context;

        public MonAnController(ShopDoAnNhanhDbcontext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var monAns = _context.MonAns.ToList();
                return View(monAns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải danh sách món ăn: " + ex.Message;
                return View(new List<MonAn>());
            }
        }

        [HttpGet]
        public IActionResult ChiTiet(int id)
        {
            try
            {
                var monAn = _context.MonAns.Find(id);
                if (monAn == null)
                {
                    TempData["Error"] = "Không tìm thấy món ăn!";
                    return RedirectToAction("Index");
                }

                return View(monAn);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải trang chi tiết món ăn: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
} 