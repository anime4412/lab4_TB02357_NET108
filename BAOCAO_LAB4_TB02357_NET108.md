# BÁO CÁO THỰC HÀNH LAB 4: PHÂN QUYỀN, XÁC THỰC & CRUD DANH MỤC (EF CORE CODE FIRST)
**MÔN HỌC: DỰ ÁN .NET 1 (NET108 / SOF1022)**  
**LỚP: DỰ ÁN 1 — NHÓM THỰC HIỆN: NHÓM 5**  
**MÃ SINH VIÊN: TB02357**  
**ĐỀ TÀI: HỆ THỐNG QUẢN LÝ THƯ VIỆN ĐIỆN TỬ (LIBRARY MANAGEMENT SYSTEM)**  

---

## MỤC TIÊU BÀI THỰC HÀNH LAB 4
1. **Xây dựng CSDL bằng EF Core Code First**: Định nghĩa Entities (`Employee`, `Category`, `Book`), `LibraryDbContext` (Fluent API + Seed Data tài khoản phân quyền) và sinh Migrations (`InitialCreate`).
2. **Xác thực dựa trên Cookie (Cookie Authentication)**: Cấu hình `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)`, thời gian sống Cookie 24 giờ, `LoginPath` và `AccessDeniedPath`.
3. **Phân quyền theo chính sách (Policy-based Authorization)**: Xây dựng 2 Policy `EmployeeOrManager` và `ManagerOnly` bảo vệ tài nguyên thư viện.
4. **Chức năng Đăng nhập / Đăng xuất hoàn chỉnh**: `AccountController` xác thực người dùng qua `EmployeeDAL` (EF Core LINQ), cấp phát `Claims` và lưu Cookie.
5. **Quản lý danh mục Thể loại sách CRUD có kiểm soát quyền + `FileUtil`**: Thủ thư và Quản lý được xem; chỉ Admin được Thêm, Sửa, Xóa và Upload ảnh qua `FileUtil`.

---

## NỘI DUNG THỰC HIỆN LAB 4 (CODE FIRST)

### PHẦN I: PHÂN QUYỀN, XÁC THỰC & CODE FIRST TẠI `Program.cs`

#### BÀI 1 (4 ĐIỂM): CẤU HÌNH `LibraryDbContext`, COOKIE AUTH & POLICY
```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.DAL;
using QuanLyThuVien.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký EF Core Code First DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// Đăng ký DI cho các lớp DAL (Scoped theo vòng đời DbContext)
builder.Services.AddScoped<EmployeeDAL>();
builder.Services.AddScoped<CategoryDAL>();

// 1. Xác thực Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

// 2. Phân quyền Policy theo vai trò
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOrManager", policy =>
        policy.RequireRole("Librarian", "Admin", "Employee", "Manager"));

    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// QUAN TRỌNG: UseAuthentication phải gọi trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

### BÀI 2 (3 ĐIỂM): CHỨC NĂNG ĐĂNG NHẬP & ĐĂNG XUẤT QUA EF CORE CODE FIRST

* `EmployeeDAL.cs` sử dụng `LibraryDbContext` và LINQ để kiểm tra tài khoản đăng nhập:
```csharp
public class EmployeeDAL
{
    private readonly LibraryDbContext _context;
    public EmployeeDAL(LibraryDbContext context) => _context = context;

    public Employee? Authenticate(string email, string password)
    {
        return _context.Employees
            .AsNoTracking()
            .FirstOrDefault(e => e.Email == email && e.Password == password && e.Active);
    }
}
```
* `AccountController.cs` tiêm `_employeeDal` qua Constructor DI, thực hiện xác thực, tạo danh sách `Claim` (`NameIdentifier`, `Name`, `Email`, `Role`, `EmployeeCode`) và ghi nhận phiên bằng `HttpContext.SignInAsync(...)`.

---

### PHẦN II: CRUD DANH MỤC THỂ LOẠI SÁCH CÓ PHÂN QUYỀN

### BÀI 3 (3 ĐIỂM): `CategoriesController` & `FileUtil`
* `Index()` & `Details(id)`: Gắn `[Authorize(Policy = "EmployeeOrManager")]` — Cả Thủ thư (`Librarian`) và Quản lý (`Admin`) đều được xem.
* `Create()`, `Edit(id)`, `Delete(id)`: Gắn `[Authorize(Policy = "ManagerOnly")]` — Chỉ Quản lý (`Admin`) được thao tác. Tài khoản Thủ thư truy cập sẽ bị chặn về `/Home/AccessDenied` (403).
* `CategoryDAL.cs`: Sử dụng `LibraryDbContext` (`Find`, `Add`, `Remove`, `SaveChanges`).
* `FileUtil.cs`: Upload ảnh bìa danh mục vào `wwwroot/uploads/categories/` có kiểm tra phần mở rộng và giới hạn 5MB.

---

## TÀI KHOẢN MẪU ĐƯỢC SEED BẰNG CODE FIRST (`HasData`)

| Tài khoản kiểm thử | Mật khẩu | Vai trò (Role) | Quyền hạn kiểm chứng |
| :--- | :--- | :--- | :--- |
| **`admin@thuvien.edu.vn`** | `Admin@123` | **Admin (Quản lý)** | **Toàn quyền**: Xem, Thêm mới (`Create`), Sửa (`Edit`), Xóa (`Delete`). |
| **`thuthu@thuvien.edu.vn`** | `Thuthu@123` | **Librarian (Thủ thư)** | **Chỉ đọc**: Xem danh mục (`Index`, `Details`). Bị chặn về trang `403 AccessDenied` khi vào Thêm/Sửa/Xóa. |

---

## KẾT LUẬN
* Toàn bộ Lab 4 đã chuyển sang kiến trúc **EF Core 8.0 Code First** kèm **Migrations** (`InitialCreate`) và Seed Data tự động (`HasData`).
* Để tạo CSDL trên SQL Server chỉ cần chạy lệnh: `dotnet ef database update` (hoặc `Update-Database` trong Package Manager Console).
* Biên dịch dự án thành công: **0 Warning(s), 0 Error(s)**.
