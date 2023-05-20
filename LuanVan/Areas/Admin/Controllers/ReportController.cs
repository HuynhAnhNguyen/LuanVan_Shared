using AspNetCoreHero.ToastNotification.Abstractions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using LuanVan.Areas.Admin.Models;
using LuanVan.Data;
using LuanVan.Models;
using LuanVan.Services;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Composition;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Helpers;
using System.Web.WebPages;


namespace LuanVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : Controller
    {
        public readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public string connectionString = "Data Source=DESKTOP-VCL1NL6;Initial Catalog=LuanVan;TrustServerCertificate=True; Integrated Security=True";
        [HttpGet]
        public IActionResult RevenueByDate()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 NgayThangNam, DoanhThu\r\nFROM (\r\n\t\tSELECT TOP 5 CONVERT(date, NgayXuatHd) AS NgayThangNam ,\r\n\t\t\tSUM(tonggiatri) AS DoanhThu\r\n\t\tFROM HoaDon\r\n\t\tWHERE TrangThaiThanhToan = 1 AND TrangThaiDonHang =2\r\n\t\tGROUP BY CONVERT(date, NgayXuatHd)\r\n\t\tORDER BY YEAR(CONVERT(date, NgayXuatHd)) DESC,\r\n\t\tMONTH(CONVERT(date, NgayXuatHd)) DESC,\r\n\t\tDAY(CONVERT(date, NgayXuatHd)) DESC)\r\nAS subquery\r\nORDER BY NgayThangNam ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult RevenueByWeek()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 N'Tuần ' + CAST(Tuan AS VARCHAR(2)) + N' Năm ' + CAST(Nam AS VARCHAR(4)) AS Tuan, DoanhThu\r\nFROM (\r\n    SELECT TOP 5 DATEPART(WEEK, NgayXuatHd) AS Tuan, YEAR(NgayXuatHd) AS Nam,\r\n\tSUM(tonggiatri) AS DoanhThu\r\n\tFROM HoaDon\r\n\tWHERE TrangThaiThanhToan = 1 AND TrangThaiDonHang =2\r\n\tGROUP BY DATEPART(WEEK, NgayXuatHd), YEAR(NgayXuatHd)\r\n\tORDER BY YEAR(NgayXuatHd) * 52 + DATEPART(WEEK, NgayXuatHd) DESC\r\n) AS subquery\r\nORDER BY Nam * 52 + Tuan ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult RevenueByMonth()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 N'Tháng ' + CAST(Thang AS VARCHAR(2)) + N' Năm ' + CAST(Nam AS VARCHAR(4)) AS Thang, DoanhThu\r\nFROM (\r\n\tSELECT TOP 5 MONTH(NgayXuatHd) AS Thang, YEAR(NgayXuatHd)AS Nam, \r\n\t\tSUM(tonggiatri) AS DoanhThu\r\n\tFROM HoaDon\r\n\tWHERE TrangThaiThanhToan = 1 AND TrangThaiDonHang =2\r\n\tGROUP BY MONTH(NgayXuatHd), YEAR(NgayXuatHd)\r\n\tORDER BY YEAR(NgayXuatHd) DESC,\r\n\t\t\tMONTH(NgayXuatHd) DESC) \r\nAS subquery\r\nORDER BY Nam ASC, Thang ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult RevenueByQuarter()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 N'Quý ' + CAST(Quy AS VARCHAR(2)) + N' Năm ' + CAST(Nam AS VARCHAR(4)) AS Quy, DoanhThu\r\nFROM (\r\n\tSELECT TOP 5 DATEPART(QUARTER, NgayXuatHd) AS Quy, + YEAR(NgayXuatHd) AS Nam, \r\n\t\tSUM(tonggiatri) AS DoanhThu\r\n\tFROM HoaDon\r\n\tWHERE TrangThaiThanhToan = 1 AND TrangThaiDonHang =2\r\n\tGROUP BY DATEPART(QUARTER, NgayXuatHd), YEAR(NgayXuatHd)\r\n\tORDER BY YEAR(NgayXuatHd) DESC, \r\n\t\tDATEPART(QUARTER, NgayXuatHd) DESC\r\n) \r\nAS subquery\r\nORDER BY Nam ASC, Quy ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult RevenueByYear()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 Nam, DoanhThu\r\nFROM (\r\n\tSELECT TOP 5 YEAR(NgayXuatHd) AS Nam, SUM(tonggiatri) AS DoanhThu\r\n\tFROM HoaDon\r\n\tWHERE TrangThaiThanhToan = 1 AND TrangThaiDonHang =2\r\n\tGROUP BY YEAR(NgayXuatHd)\r\n\tORDER BY YEAR(NgayXuatHd) DESC\r\n) \r\nAS subquery\r\nORDER BY Nam ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult InvoiceSuccessOrFailure()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 NgayThangNam, SoLuongThanhCong, SoLuongThatBai, TongHoaDon\r\nFROM (\r\n    SELECT TOP 5 CONVERT(date, NgayXuatHd) AS NgayThangNam,\r\n\t\tSUM(CASE WHEN TrangThaiThanhToan = 1 THEN 1 ELSE 0 END) AS SoLuongThanhCong,\r\n\t\tSUM(CASE WHEN TrangThaiThanhToan != 1 THEN 1 ELSE 0 END) AS SoLuongThatBai,\r\n\t\tCOUNT(MaHoaDon) AS TongHoaDon\r\n\tFROM HoaDon\r\n\tGROUP BY CONVERT(date, NgayXuatHd)\r\n\tORDER BY \r\n\t\tYEAR(CONVERT(date, NgayXuatHd)) DESC,\r\n\t\tMONTH(CONVERT(date, NgayXuatHd)) DESC,\r\n\t\tDAY(CONVERT(date, NgayXuatHd)) DESC\r\n) AS subquery\r\nORDER BY NgayThangNam ASC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GetPayByUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT TOP 5 NgayThangNam, ThanhToanBoiKH, ThanhToanBoiGuest, TongHoaDon\r\nFROM (\r\n    SELECT TOP 5 CONVERT(date, NgayXuatHd) AS NgayThangNam, \r\n        SUM(CASE WHEN KhachHangId IS NOT NULL THEN 1 ELSE 0 END) AS ThanhToanBoiKH,\r\n        SUM(CASE WHEN KhachHangId IS NULL THEN 1 ELSE 0 END) AS ThanhToanBoiGuest,\r\n        COUNT(MaHoaDon) AS TongHoaDon\r\n    FROM HoaDon\r\n    GROUP BY CONVERT(date, NgayXuatHd)\r\n\tORDER BY \r\n\tYEAR(CONVERT(date, NgayXuatHd)) DESC,\r\n\tMONTH(CONVERT(date, NgayXuatHd)) DESC,\r\n\tDAY(CONVERT(date, NgayXuatHd)) DESC\r\n) AS subquery\r\nORDER BY NgayThangNam ASC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GetAgeUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT DATEDIFF(YEAR, NgaySinh, GETDATE()) AS DoTuoi, COUNT(NgaySinh) AS SoLuong\r\nFROM KhachHang\r\nGROUP BY DATEDIFF(YEAR, NgaySinh, GETDATE())";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GetUsersByGender()
        {
            // Kết nối cơ sở dữ liệu
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT GioiTinh, COUNT(GioiTinh) AS SoLuong\r\nFROM KhachHang\r\nGROUP BY GioiTinh";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GetProductByProducer()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT a.TenNsx, COUNT(b.TenSanPham) AS SoLuong\r\nFROM NhaSanXuat a LEFT JOIN SanPham b ON a.MaNsx= b.MaNsx\r\nGROUP BY a.TenNsx\r\nORDER BY a.TenNsx";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GetProductByType()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT c.TenLoaiSp, COUNT(p.TenSanPham) AS SoLuong\r\nFROM LoaiSanPham c\r\nLEFT JOIN SanPham p ON c.MaLoaiSp = p.MaLoaiSp\r\nGROUP BY c.TenLoaiSp\r\nORDER BY c.TenLoaiSp";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult PercentByPayment()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT a.TenPttt AS TenPTTT, ROUND((COUNT(hd.MaHoaDon)* 100.0 / (SELECT COUNT(*) FROM HoaDon)),2) as PhanTram\r\nFROM ThanhToan a\r\nJOIN HoaDon hd ON a.MaPttt = hd.MaPttt\r\nGROUP BY a.TenPttt";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Tạo mảng chứa dữ liệu
                        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        // Trả về dữ liệu dưới dạng JSON
                        return Json(data);
                    }
                }
            }
        }

    }
}
