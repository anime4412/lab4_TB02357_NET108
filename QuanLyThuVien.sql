-- ==============================================================================
-- BÀI THỰC HÀNH LAB 4: PHÂN QUYỀN, XÁC THỰC VÀ QUẢN LÝ DANH MỤC CRUD
-- MÔN HỌC: DỰ ÁN .NET 1 (NET108 / SOF1022)
-- SINH VIÊN: TB02357 | NHÓM: NHÓM 5
-- ĐỀ TÀI: HỆ THỐNG QUẢN LÝ THƯ VIỆN ĐIỆN TỬ (LIBRARY MANAGEMENT SYSTEM)
-- ==============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyThuVien')
BEGIN
    CREATE DATABASE QuanLyThuVien;
END
GO

USE QuanLyThuVien;
GO

-- 1. Bảng Nhân viên / Tài khoản hệ thống (Employees)
IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE Employees (
        Id INT PRIMARY KEY IDENTITY(1,1),
        EmployeeCode VARCHAR(20) UNIQUE NOT NULL,
        FullName NVARCHAR(150) NOT NULL,
        Email VARCHAR(100) UNIQUE NOT NULL,
        Password VARCHAR(255) NOT NULL,
        Phone VARCHAR(15),
        Role NVARCHAR(50) NOT NULL DEFAULT N'Librarian', -- 'Admin' (Manager) hoặc 'Librarian' (Employee)
        Active BIT DEFAULT 1
    );
END
GO

-- 2. Bảng Thể loại sách (Categories)
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Categories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500),
        Image NVARCHAR(255),
        Active BIT DEFAULT 1
    );
END
GO

-- 3. Bảng Sách (Books)
IF OBJECT_ID('dbo.Books', 'U') IS NULL
BEGIN
    CREATE TABLE Books (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ISBN VARCHAR(20) UNIQUE NOT NULL,
        Title NVARCHAR(250) NOT NULL,
        CategoryId INT FOREIGN KEY REFERENCES Categories(Id) ON DELETE SET NULL,
        Author NVARCHAR(150),
        PublishYear INT DEFAULT 2024,
        Price DECIMAL(18,2) DEFAULT 0,
        Quantity INT DEFAULT 1,
        AvailableQuantity INT DEFAULT 1,
        CoverImage NVARCHAR(255),
        Description NVARCHAR(MAX),
        Active BIT DEFAULT 1
    );
END
GO

-- NẠP DỮ LIỆU MẪU ĐĂNG NHẬP VÀ PHÂN QUYỀN
-- Mật khẩu demo: Admin@123 và Thuthu@123 (lưu chuỗi băm chuẩn)
IF NOT EXISTS (SELECT 1 FROM Employees WHERE Email = 'admin@thuvien.edu.vn')
BEGIN
    INSERT INTO Employees (EmployeeCode, FullName, Email, Password, Phone, Role, Active) VALUES
    ('NV_ADMIN', N'Nguyễn Quản Trị (Admin)', 'admin@thuvien.edu.vn', 'Admin@123', '0911223344', 'Admin', 1),
    ('NV_THUTHU', N'Trần Thị Thủ Thư (Staff)', 'thuthu@thuvien.edu.vn', 'Thuthu@123', '0988776655', 'Librarian', 1);
END
GO

-- Dữ liệu mẫu danh mục sách
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Công nghệ thông tin')
BEGIN
    INSERT INTO Categories (Name, Description, Image, Active) VALUES
    (N'Công nghệ thông tin', N'Sách chuyên ngành Lập trình C#, ASP.NET Core, Cơ sở dữ liệu', 'it_category.jpg', 1),
    (N'Kinh tế & Khởi nghiệp', N'Sách Quản trị kinh doanh, Tài chính doanh nghiệp', 'kinhte_category.jpg', 1),
    (N'Văn học & Nghệ thuật', N'Tiểu thuyết, truyện ngắn, tác phẩm kinh điển', 'vanhoc_category.jpg', 1),
    (N'Kỹ năng sống', N'Phát triển bản thân, tư duy lãnh đạo, giao tiếp', 'kynang_category.jpg', 1),
    (N'Khoa học cơ bản', N'Toán học, Vật lý, Thiên văn học', 'khoahoc_category.jpg', 1);
END
GO

-- Dữ liệu mẫu sách
IF NOT EXISTS (SELECT 1 FROM Books WHERE ISBN = '9780132350884')
BEGIN
    INSERT INTO Books (ISBN, Title, CategoryId, Author, PublishYear, Price, Quantity, AvailableQuantity, CoverImage, Description, Active) VALUES
    ('9780132350884', N'Clean Code: A Handbook of Agile Software Craftsmanship', 1, N'Robert C. Martin', 2021, 250000, 10, 8, 'cleancode.jpg', N'Sách gối đầu giường của mọi lập trình viên chuyên nghiệp.', 1),
    ('9780201616224', N'The Pragmatic Programmer', 1, N'Andrew Hunt & David Thomas', 2022, 280000, 8, 7, 'pragmatic.jpg', N'Hành trình từ lập trình viên nghiệp dư trở thành bậc thầy phát triển phần mềm.', 1),
    ('9786045882885', N'Đắc Nhân Tâm', 4, N'Dale Carnegie', 2023, 95000, 15, 14, 'dacnhantam.jpg', N'Nghệ thuật thu phục lòng người kinh điển.', 1);
END
GO

SELECT 'Employees' AS Bang, COUNT(*) AS SoLuong FROM Employees
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Books', COUNT(*) FROM Books;
GO
