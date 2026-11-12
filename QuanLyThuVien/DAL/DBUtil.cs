using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyThuVien.DAL
{
    public static class DBUtil
    {
        public static string connString = "Server=localhost;Database=QuanLyThuVien;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        public static DataTable queryDataTable(string sql, List<object>? parameters = null)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            for (int i = 0; i < parameters.Count; i++)
                            {
                                var paramName = $"@{i + 1}";
                                var value = parameters[i] ?? DBNull.Value;
                                if (value is string)
                                {
                                    var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                    p.Value = value;
                                    cmd.Parameters.Add(p);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(paramName, value);
                                }
                            }
                        }
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi QueryDataTable: {ex.Message}");
                    throw;
                }
            }
            return dataTable;
        }

        public static object? executeScalar(string sql, List<object>? parameters = null)
        {
            object? result = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            for (int i = 0; i < parameters.Count; i++)
                            {
                                var paramName = $"@{i + 1}";
                                var value = parameters[i] ?? DBNull.Value;
                                if (value is string)
                                {
                                    var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                    p.Value = value;
                                    cmd.Parameters.Add(p);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(paramName, value);
                                }
                            }
                        }
                        result = cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi executeScalar: {ex.Message}");
                    throw;
                }
            }
            return result;
        }

        public static int executeNonQuery(string sql, List<object>? parameters = null)
        {
            int rowsAffected = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            for (int i = 0; i < parameters.Count; i++)
                            {
                                var paramName = $"@{i + 1}";
                                var value = parameters[i] ?? DBNull.Value;
                                if (value is string)
                                {
                                    var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                    p.Value = value;
                                    cmd.Parameters.Add(p);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(paramName, value);
                                }
                            }
                        }
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi executeNonQuery: {ex.Message}");
                    throw;
                }
            }
            return rowsAffected;
        }
    }
}
