// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Tạo ViewModel phục vụ Dashboard và Thống kê (Module 4)

using System.Collections.Generic;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels
{
    public class ThongKeLoaiXe
    {
        public string TenLoai { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class ThongKeTrangThai
    {
        public string TrangThai { get; set; } = "";
        public int SoLuong { get; set; }
    }

    public class ThongKeLuotGuiViTri
    {
        public string TenViTri { get; set; } = "";
        public int SoLuot { get; set; }
    }

    public class ThongKeLuotGuiChuXe
    {
        public string HoTen { get; set; } = "";
        public int SoLuot { get; set; }
    }

    public class ThongKeTheoThang
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int SoLuot { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class ThongKeViewModel
    {
        public DashboardViewModel Dashboard { get; set; } = new DashboardViewModel();
        public List<ThongKeLoaiXe> SoPhuongTienTheoLoai { get; set; } = new List<ThongKeLoaiXe>();
        public List<ThongKeTrangThai> SoViTriTheoTrangThai { get; set; } = new List<ThongKeTrangThai>();
        public List<ThongKeTrangThai> SoPhieuTheoTrangThai { get; set; } = new List<ThongKeTrangThai>();
        public List<ThongKeLoaiXe> LuotGuiTheoLoaiXe { get; set; } = new List<ThongKeLoaiXe>();
        public List<ThongKeLuotGuiViTri> LuotGuiTheoViTri { get; set; } = new List<ThongKeLuotGuiViTri>();
        public ThongKeLuotGuiViTri? ViTriDungNhieuNhat { get; set; }
        public List<ThongKeLuotGuiChuXe> LuotGuiTheoChuXe { get; set; } = new List<ThongKeLuotGuiChuXe>();
        public List<ThongKeTheoThang> LuotGuiVaDoanhThuTheoThang { get; set; } = new List<ThongKeTheoThang>();
    }
}
