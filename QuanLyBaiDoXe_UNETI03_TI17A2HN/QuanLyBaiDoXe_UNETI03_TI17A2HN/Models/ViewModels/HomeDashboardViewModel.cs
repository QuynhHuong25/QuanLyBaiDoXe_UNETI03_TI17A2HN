// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Tạo ViewModel phục vụ Trang chủ Dashboard tổng quan (Module 4)

using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public List<PhieuGuiXe> HoatDongGanDay { get; set; } = new();
        public List<LoaiPhuongTien> DanhSachLoaiXe { get; set; } = new();
        public int TongXeDangGui { get; set; }
        public int TongViTriTrong { get; set; }
        public int TongKhachHang { get; set; }
        public int TongPhuongTien { get; set; }
    }
}
