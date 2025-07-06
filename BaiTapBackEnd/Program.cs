using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BaiTapBackEnd.Models; // Thay b?ng namespace th?c t? c?a b?n

var builder = WebApplication.CreateBuilder(args);

// 1. ��ng k? DbContext
builder.Services.AddDbContext<ShopDoAnNhanhDbcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Razor + Runtime Compilation
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Cho ph�p hot reload Razor

// 3. C?u h?nh Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// 4. C?u h?nh Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 5. Middleware x? l? l?i
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage(); // R? l?i h�n khi dev
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. K?ch ho?t Session (n?u dng)
app.UseSession();

// 7. K?ch ho?t xc th?c v phn quy?n
app.UseAuthentication();
app.UseAuthorization();

// 8. Routing cho Areas v Default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
