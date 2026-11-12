using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.DAL;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Controllers
{
    /// <summary>
    /// BÀI 2 (3 ĐIỂM): XÂY DỰNG CHỨC NĂNG ĐĂNG NHẬP VÀ ĐĂNG XUẤT VỚI COOKIE
    /// </summary>
    public class AccountController : Controller
    {
        private readonly EmployeeDAL _employeeDal;

        public AccountController(EmployeeDAL employeeDal)
        {
            _employeeDal = employeeDal;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Đăng nhập hệ thống";
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            model.ReturnUrl = returnUrl ?? model.ReturnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tài khoản từ CSDL SQL Server qua EmployeeDAL
            var user = EmployeeDAL.Authenticate(model.Email, model.Password);

            if (user != null)
            {
                // Chuẩn hóa tên vai trò phù hợp với Policy
                var roleName = user.Role; // "Admin" hoặc "Librarian"

                // Thiết lập các Claim danh tính
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("EmployeeCode", user.EmployeeCode ?? string.Empty)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddHours(24) : null
                };

                // Đăng nhập và tạo Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                // Chuyển hướng an toàn
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc Mật khẩu không chính xác hoặc tài khoản đã bị khóa.");
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
    }
}
