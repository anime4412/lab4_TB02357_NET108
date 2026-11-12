using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyThuVien.DAL;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình kết nối CSDL cho DBUtil từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    DBUtil.connString = connectionString;
}

builder.Services.AddControllersWithViews();

// Đăng ký DI cho các lớp DAL
builder.Services.AddTransient<EmployeeDAL>();
builder.Services.AddTransient<CategoryDAL>();

// ==============================================================================
// BÀI 1 (4 ĐIỂM): CẤU HÌNH XÁC THỰC VÀ PHÂN QUYỀN
// ==============================================================================

// 1. Xác thực: Dựa trên Cookie (Cookie Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";              // Đường dẫn khi chưa đăng nhập
        options.AccessDeniedPath = "/Home/AccessDenied";   // Đường dẫn khi bị từ chối quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(24);   // Thời gian tồn tại của Cookie là 24 giờ
    });

// 2. Phân quyền: Thiết lập các Policy theo vai trò người dùng
builder.Services.AddAuthorization(options =>
{
    // Policy cho phép cả Thủ thư (Employee/Librarian) và Quản lý (Manager/Admin)
    options.AddPolicy("EmployeeOrManager", policy =>
        policy.RequireRole("Librarian", "Admin", "Employee", "Manager"));

    // Policy chỉ cho phép Quản lý / Giám đốc / Admin
    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

// Cấu hình HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ==============================================================================
// QUAN TRỌNG: Xác thực phải được gọi TRƯỚC Phân quyền
// ==============================================================================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
