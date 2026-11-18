using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Data
{
    /// <summary>
    /// CODE FIRST DBCONTEXT CHO LAB 4: Ánh xạ CSDL QuanLyThuVien & Seed dữ liệu phân quyền
    /// </summary>
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc Unique Index
            modelBuilder.Entity<Employee>().HasIndex(e => e.EmployeeCode).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<Book>().HasIndex(b => b.ISBN).IsUnique();

            // Quan hệ Category - Book
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed Data mặc định bằng Code First HasData
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    EmployeeCode = "NV_ADMIN",
                    FullName = "Nguyễn Quản Trị (Admin)",
                    Email = "admin@thuvien.edu.vn",
                    Password = "Admin@123",
                    Phone = "0911223344",
                    Role = "Admin",
                    Active = true
                },
                new Employee
                {
                    Id = 2,
                    EmployeeCode = "NV_THUTHU",
                    FullName = "Trần Thị Thủ Thư (Staff)",
                    Email = "thuthu@thuvien.edu.vn",
                    Password = "Thuthu@123",
                    Phone = "0988776655",
                    Role = "Librarian",
                    Active = true
                }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Công nghệ thông tin", Description = "Sách chuyên ngành Lập trình C#, ASP.NET Core, Cơ sở dữ liệu", Image = "it_category.jpg", Active = true },
                new Category { Id = 2, Name = "Kinh tế & Khởi nghiệp", Description = "Sách Quản trị kinh doanh, Tài chính doanh nghiệp", Image = "kinhte_category.jpg", Active = true },
                new Category { Id = 3, Name = "Văn học & Nghệ thuật", Description = "Tiểu thuyết, truyện ngắn, tác phẩm kinh điển", Image = "vanhoc_category.jpg", Active = true },
                new Category { Id = 4, Name = "Kỹ năng sống", Description = "Phát triển bản thân, tư duy lãnh đạo, giao tiếp", Image = "kynang_category.jpg", Active = true },
                new Category { Id = 5, Name = "Khoa học cơ bản", Description = "Toán học, Vật lý, Thiên văn học", Image = "khoahoc_category.jpg", Active = true }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, ISBN = "9780132350884", Title = "Clean Code: A Handbook of Agile Software Craftsmanship", CategoryId = 1, Author = "Robert C. Martin", PublishYear = 2021, Price = 250000, Quantity = 10, AvailableQuantity = 8, CoverImage = "cleancode.jpg", Description = "Sách gối đầu giường của mọi lập trình viên chuyên nghiệp.", Active = true },
                new Book { Id = 2, ISBN = "9780201616224", Title = "The Pragmatic Programmer", CategoryId = 1, Author = "Andrew Hunt & David Thomas", PublishYear = 2022, Price = 280000, Quantity = 8, AvailableQuantity = 7, CoverImage = "pragmatic.jpg", Description = "Hành trình từ lập trình viên nghiệp dư trở thành bậc thầy phát triển phần mềm.", Active = true },
                new Book { Id = 3, ISBN = "9786045882885", Title = "Đắc Nhân Tâm", CategoryId = 4, Author = "Dale Carnegie", PublishYear = 2023, Price = 95000, Quantity = 15, AvailableQuantity = 14, CoverImage = "dacnhantam.jpg", Description = "Nghệ thuật thu phục lòng người kinh điển.", Active = true }
            );
        }
    }
}
