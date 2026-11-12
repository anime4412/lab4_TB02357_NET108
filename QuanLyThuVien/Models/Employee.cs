namespace QuanLyThuVien.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "Librarian"; // "Admin" (Manager) hoặc "Librarian" (Employee)
        public bool Active { get; set; } = true;
    }
}
