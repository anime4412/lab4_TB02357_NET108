using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.DAL;
using QuanLyThuVien.Models;
using QuanLyThuVien.Utils;

namespace QuanLyThuVien.Controllers
{
    /// <summary>
    /// PHẦN II - BÀI 3 (3 ĐIỂM): QUẢN LÝ DANH MỤC THỂ LOẠI SÁCH CRUD VỚI PHÂN QUYỀN CHẶT CHẼ
    /// • Thủ thư (Librarian) và Quản lý (Admin): Được Xem danh sách và chi tiết (EmployeeOrManager)
    /// • Chỉ Quản lý (Admin): Được Thêm, Sửa, Xóa (ManagerOnly)
    /// </summary>
    public class CategoriesController : Controller
    {
        private readonly CategoryDAL _categoryDal;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(CategoryDAL categoryDal, IWebHostEnvironment webHostEnvironment)
        {
            _categoryDal = categoryDal;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Categories/
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Index()
        {
            var categories = _categoryDal.GetAll();
            return View(categories);
        }

        // GET: /Categories/Details/5
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Details(int id)
        {
            var category = _categoryDal.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // GET: /Categories/Create
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tải lên ảnh nếu có qua FileUtil
                    if (imageFile != null)
                    {
                        category.Image = await FileUtil.UploadFileAsync(imageFile, "categories", _webHostEnvironment.WebRootPath);
                    }

                    int newId = _categoryDal.Insert(category);
                    if (newId > 0)
                    {
                        TempData["Success"] = "Thêm thể loại sách mới thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu thể loại: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: /Categories/Edit/5
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Edit(int id)
        {
            var category = _categoryDal.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? imageFile)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        // Upload ảnh mới
                        category.Image = await FileUtil.UploadFileAsync(imageFile, "categories", _webHostEnvironment.WebRootPath);
                    }

                    bool updated = _categoryDal.Update(category);
                    if (updated)
                    {
                        TempData["Success"] = "Cập nhật thể loại sách thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật thể loại: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: /Categories/Delete/5
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Delete(int id)
        {
            var category = _categoryDal.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var existing = _categoryDal.GetById(id);
                if (existing != null && !string.IsNullOrEmpty(existing.Image))
                {
                    FileUtil.DeleteFile(existing.Image, _webHostEnvironment.WebRootPath);
                }

                _categoryDal.Delete(id);
                TempData["Success"] = "Xóa thể loại sách thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa thể loại sách: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
