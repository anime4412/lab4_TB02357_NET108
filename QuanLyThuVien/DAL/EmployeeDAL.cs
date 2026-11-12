using System;
using System.Collections.Generic;
using System.Data;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.DAL
{
    public class EmployeeDAL
    {
        /// <summary>
        /// BÀI 2: Phương thức xác thực thông tin đăng nhập nhân viên/thủ thư
        /// </summary>
        public static Employee? Authenticate(string email, string password)
        {
            string sql = @"SELECT Id, EmployeeCode, FullName, Email, Password, Phone, Role, Active 
                           FROM Employees 
                           WHERE Email = @1 AND Password = @2 AND Active = 1";
            DataTable dt = DBUtil.queryDataTable(sql, new List<object> { email, password });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Employee
                {
                    Id = Convert.ToInt32(row["Id"]),
                    EmployeeCode = row["EmployeeCode"].ToString() ?? string.Empty,
                    FullName = row["FullName"].ToString() ?? string.Empty,
                    Email = row["Email"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : null,
                    Role = row["Role"].ToString() ?? "Librarian",
                    Active = Convert.ToBoolean(row["Active"])
                };
            }
            return null;
        }

        public Employee? GetById(int id)
        {
            string sql = "SELECT Id, EmployeeCode, FullName, Email, Password, Phone, Role, Active FROM Employees WHERE Id = @1";
            DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Employee
                {
                    Id = Convert.ToInt32(row["Id"]),
                    EmployeeCode = row["EmployeeCode"].ToString() ?? string.Empty,
                    FullName = row["FullName"].ToString() ?? string.Empty,
                    Email = row["Email"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : null,
                    Role = row["Role"].ToString() ?? "Librarian",
                    Active = Convert.ToBoolean(row["Active"])
                };
            }
            return null;
        }

        public List<Employee> GetAll()
        {
            List<Employee> list = new List<Employee>();
            string sql = "SELECT Id, EmployeeCode, FullName, Email, Password, Phone, Role, Active FROM Employees ORDER BY Id ASC";
            DataTable dt = DBUtil.queryDataTable(sql);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Employee
                {
                    Id = Convert.ToInt32(row["Id"]),
                    EmployeeCode = row["EmployeeCode"].ToString() ?? string.Empty,
                    FullName = row["FullName"].ToString() ?? string.Empty,
                    Email = row["Email"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : null,
                    Role = row["Role"].ToString() ?? "Librarian",
                    Active = Convert.ToBoolean(row["Active"])
                });
            }
            return list;
        }
    }
}
