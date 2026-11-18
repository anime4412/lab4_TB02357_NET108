using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Data;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.DAL
{
    public class EmployeeDAL
    {
        private readonly LibraryDbContext _context;

        public EmployeeDAL(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// BÀI 2: Phương thức xác thực thông tin đăng nhập nhân viên/thủ thư bằng EF Core Code First
        /// </summary>
        public Employee? Authenticate(string email, string password)
        {
            return _context.Employees
                .AsNoTracking()
                .FirstOrDefault(e => e.Email == email && e.Password == password && e.Active);
        }

        public Employee? GetById(int id)
        {
            return _context.Employees.Find(id);
        }

        public List<Employee> GetAll()
        {
            return _context.Employees
                .OrderBy(e => e.Id)
                .AsNoTracking()
                .ToList();
        }
    }
}
