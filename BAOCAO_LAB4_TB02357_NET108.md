# BÁO CÁO THỰC HÀNH LAB 4: PHÂN QUYỀN, XÁC THỰC & CRUD DANH MỤC
**MÔN HỌC: DỰ ÁN .NET 1 (NET108 / SOF1022)**  
**LỚP: DỰ ÁN 1 — NHÓM THỰC HIỆN: NHÓM 5**  
**MÃ SINH VIÊN: TB02357**  
**ĐỀ TÀI: HỆ THỐNG QUẢN LÝ THƯ VIỆN ĐIỆN TỬ (LIBRARY MANAGEMENT SYSTEM)**  

---

## MỤC TIÊU BÀI THỰC HÀNH LAB 4
1. **Xác thực dựa trên Cookie (Cookie Authentication)**: Đăng ký cơ chế xác thực phiên người dùng, cấu hình thời gian sống của Cookie 24 giờ và xử lý chuyển hướng khi chưa đăng nhập.
2. **Phân quyền theo chính sách (Policy-based Authorization)**: Xây dựng các Policy phân cấp vai trò (`EmployeeOrManager` và `ManagerOnly`) để bảo vệ các tài nguyên hệ thống thư viện.
3. **Chức năng Đăng nhập / Đăng xuất hoàn chỉnh**: Đóng gói danh tính người dùng bằng danh sách `Claim` (`NameIdentifier`, `Name`, `Email`, `Role`), hỗ trợ ghi nhớ đăng nhập `RememberMe` và đăng xuất an toàn.
4. **Quản lý danh mục Thể loại sách CRUD có kiểm soát quyền**: Phân quyền chi tiết: Thủ thư và Quản lý đều được xem danh sách và chi tiết thể loại; chỉ Quản lý/Admin mới có quyền Thêm, Sửa, Xóa.
5. **Tiện ích tải lên hình ảnh (`FileUtil`)**: Hỗ trợ upload ảnh bìa danh mục vào `wwwroot/uploads/categories/` có kiểm tra định dạng và kích thước file an toàn.

---

## NỘI DUNG THỰC HIỆN LAB 4 (ĐỀ TÀI THƯ VIỆN ĐIỆN TỬ)

### PHẦN I: PHÂN QUYỀN VÀ ĐĂNG NHẬP

### BÀI 1 (4 ĐIỂM): CẤU HÌNH XÁC THỰC VÀ PHÂN QUYỀN TẠI `Program.cs`

#### 1. Cấu hình dịch vụ Xác thực Cookie
Tại `Program.cs`, cơ chế Cookie Authentication được đăng ký với các tham số điều hướng bảo mật:
* `LoginPath = "/Account/Login"`: Đường dẫn chuyển hướng khi người dùng chưa đăng nhập cố tình truy cập vào trang bảo vệ.
* `AccessDeniedPath = "/Home/AccessDenied"`: Đường dẫn trang thông báo lỗi 403 khi tài khoản không đủ quyền hạn.
* `ExpireTimeSpan = TimeSpan.FromHours(24)`: Thiết lập thời gian sống của Cookie danh tính là 24 giờ.

#### 2. Cấu hình các Policy phân quyền theo vai trò
* **Policy `EmployeeOrManager`**: Yêu cầu người dùng có một trong các vai trò `Librarian` (Thủ thư) hoặc `Admin` (Quản lý). Dùng cho các chức năng tra cứu, xem danh sách.
* **Policy `ManagerOnly`**: Yêu cầu người dùng bắt buộc phải có vai trò `Admin` (Quản lý). Dùng cho các chức năng quản trị nhạy cảm: Thêm mới, Chỉnh sửa, Xóa dữ liệu.

#### 3. Thứ tự Middleware Pipeline
Hai middleware quan trọng được gọi theo thứ tự nghiêm ngặt:
```csharp
// QUAN TRỌNG: UseAuthentication phải đứng trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
```

#### 4. Mã nguồn chi tiết cấu hình tại `Program.cs`
```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyThuVien.DAL;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình chuỗi kết nối DBUtil từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    DBUtil.connString = connectionString;
}

builder.Services.AddControllersWithViews();

// Đăng ký Dependency Injection cho các lớp DAL
builder.Services.AddTransient<EmployeeDAL>();
builder.Services.AddTransient<CategoryDAL>();

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
    // Cho phép cả Thủ thư và Admin
    options.AddPolicy("EmployeeOrManager", policy =>
        policy.RequireRole("Librarian", "Admin", "Employee", "Manager"));

    // Chỉ cho phép Admin/Quản lý
    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// QUAN TRỌNG: Xác thực phải gọi trước Phân quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

### BÀI 2 (3 ĐIỂM): XÂY DỰNG CHỨC NĂNG ĐĂNG NHẬP VÀ ĐĂNG XUẤT

#### 1. Xây dựng `LoginViewModel.cs`
Đóng gói thông tin người dùng gửi lên từ form đăng nhập:
```csharp
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập (24h)")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}
```

#### 2. Xử lý logic tại `AccountController.cs`
* Xác thực thông tin tài khoản thông qua hàm `EmployeeDAL.Authenticate(email, password)`.
* Cấp phát danh sách các `Claim` danh tính: `NameIdentifier`, `Name`, `Email`, `Role`.
* Ghi nhận phiên làm việc vào Cookie bằng `HttpContext.SignInAsync(...)`.
* Xử lý đăng xuất an toàn bằng `HttpContext.SignOutAsync(...)`.

```csharp
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
{
    model.ReturnUrl = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");
    if (!ModelState.IsValid) return View(model);

    // Kiểm tra thông tin đăng nhập trong CSDL SQL Server
    var user = EmployeeDAL.Authenticate(model.Email, model.Password);

    if (user != null)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role), // 'Admin' hoặc 'Librarian'
            new Claim("EmployeeCode", user.EmployeeCode ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddHours(24) : null
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        if (Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    ModelState.AddModelError(string.Empty, "Email hoặc Mật khẩu không chính xác.");
    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
}
```

---

### PHẦN II: CRUD DANH MỤC CƠ BẢN VÀ PHÂN QUYỀN

### BÀI 3 (3 ĐIỂM): QUẢN LÝ DANH MỤC THỂ LOẠI SÁCH VỚI PHÂN QUYỀN CHẶT CHẼ

#### 1. Áp dụng Phân quyền Policy tại `CategoriesController.cs`
* **Xem danh sách (`Index`) & Chi tiết (`Details`)**: Gắn `[Authorize(Policy = "EmployeeOrManager")]`.
  - Cả tài khoản Thủ thư (`thuthu@thuvien.edu.vn`) và Admin (`admin@thuvien.edu.vn`) đều được phép truy cập.
* **Thêm mới (`Create`)**: Gắn `[Authorize(Policy = "ManagerOnly")]`.
  - Chỉ Quản lý/Admin mới có quyền tạo thể loại sách mới.
* **Chỉnh sửa (`Edit`)**: Gắn `[Authorize(Policy = "ManagerOnly")]`.
  - Chỉ Quản lý/Admin mới có quyền cập nhật thông tin và thay đổi ảnh thể loại.
* **Xóa (`Delete` & `DeleteConfirmed`)**: Gắn `[Authorize(Policy = "ManagerOnly")]`.
  - Chỉ Quản lý/Admin mới có quyền xóa thể loại khỏi hệ thống.

#### 2. Tích hợp tiện ích tải ảnh `FileUtil`
Khi người dùng thêm hoặc sửa thể loại sách, hình ảnh đại diện được upload an toàn thông qua hàm `FileUtil.UploadFileAsync(imageFile, "categories", webRootPath)`.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Policy = "ManagerOnly")]
public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
{
    if (ModelState.IsValid)
    {
        if (imageFile != null)
        {
            category.Image = await FileUtil.UploadFileAsync(imageFile, "categories", _webHostEnvironment.WebRootPath);
        }
        int newId = _categoryDal.Insert(category);
        if (newId > 0)
        {
            TempData["Success"] = "Thêm thể loại sách thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
    return View(category);
}
```

---

## KỊCH BẢN KIỂM THỬ PHÂN QUYỀN THỰC TẾ
Hệ thống CSDL đã được nạp sẵn 2 tài khoản mẫu để kiểm thử:

| Tài khoản kiểm thử | Mật khẩu | Vai trò (Role) | Quyền hạn kiểm chứng |
| :--- | :--- | :--- | :--- |
| **`admin@thuvien.edu.vn`** | `Admin@123` | **Admin (Quản lý)** | **Toàn quyền**: Xem danh mục, Thêm mới (`Create`), Sửa (`Edit`), Xóa (`Delete`). |
| **`thuthu@thuvien.edu.vn`** | `Thuthu@123` | **Librarian (Thủ thư)** | **Chỉ đọc**: Xem danh mục (`Index`), xem chi tiết (`Details`). Nếu cố tình truy cập vào `/Categories/Create`, hệ thống lập tức chặn lại và chuyển hướng về trang lỗi **`403 AccessDenied`**. |

---

## KẾT LUẬN ĐÁNH GIÁ KẾT QUẢ LAB 4
* **Bài 1 (4/4 điểm)**: Cấu hình Cookie Authentication và 2 Policy phân quyền vai trò đầy đủ, middleware gọi đúng thứ tự.
* **Bài 2 (3/3 điểm)**: Hoàn thiện chức năng đăng nhập, đăng xuất, cấp phát Claim và trang báo lỗi từ chối quyền `AccessDenied`.
* **Bài 3 (3/3 điểm)**: Cài đặt đầy đủ 5 thao tác CRUD cho Thể loại sách có kiểm soát Policy chặt chẽ, tích hợp upload ảnh qua `FileUtil`.
* Dự án biên dịch thành công: **0 Lỗi (0 Error), 0 Cảnh báo (0 Warning)**.
* Toàn bộ mã nguồn và báo cáo được lưu trữ độc lập tại: `D:\test\lab4_TB02357_NET108\`.
