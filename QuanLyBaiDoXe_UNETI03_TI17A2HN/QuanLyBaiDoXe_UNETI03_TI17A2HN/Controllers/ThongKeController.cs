// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Tạo Controller xử lý Thống kê và Dashboard (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin")]
    public class ThongKeController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public ThongKeController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ThongKeViewModel();

            // 1. Dashboard (Tổng quan)
            viewModel.Dashboard.TongLoaiPhuongTien = await _context.LoaiPhuongTien.CountAsync();
            viewModel.Dashboard.TongViTriDo = await _context.ViTriDoXe.CountAsync();
            viewModel.Dashboard.TongChuPhuongTien = await _context.ChuPhuongTien.CountAsync();
            viewModel.Dashboard.TongPhuongTien = await _context.PhuongTien.CountAsync();
            viewModel.Dashboard.TongPhieuGuiXe = await _context.PhieuGuiXe.CountAsync();

            viewModel.Dashboard.SoPhieuChoXacNhan = await _context.PhieuGuiXe.CountAsync(p => p.TrangThai == "Chờ xác nhận");
            viewModel.Dashboard.SoXeDangGui = await _context.PhieuGuiXe.CountAsync(p => p.TrangThai == "Đang gửi");

            viewModel.Dashboard.SoViTriConTrong = await _context.ViTriDoXe.CountAsync(v => v.TrangThai == "Còn trống");
            viewModel.Dashboard.SoViTriDangSuDung = await _context.ViTriDoXe.CountAsync(v => v.TrangThai == "Đang sử dụng");

            // 2. Thống kê chi tiết bằng LINQ
            // 2.1 Số phương tiện theo từng loại
            viewModel.SoPhuongTienTheoLoai = await _context.PhuongTien
                .Include(p => p.LoaiPhuongTien)
                .GroupBy(p => p.LoaiPhuongTien.TenLoaiPhuongTien)
                .Select(g => new ThongKeLoaiXe
                {
                    TenLoai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.2 Số vị trí theo từng trạng thái
            viewModel.SoViTriTheoTrangThai = await _context.ViTriDoXe
                .GroupBy(v => v.TrangThai)
                .Select(g => new ThongKeTrangThai
                {
                    TrangThai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.3 Số phiếu theo từng trạng thái
            viewModel.SoPhieuTheoTrangThai = await _context.PhieuGuiXe
                .GroupBy(p => p.TrangThai)
                .Select(g => new ThongKeTrangThai
                {
                    TrangThai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.4 Số lượt gửi theo từng loại phương tiện
            viewModel.LuotGuiTheoLoaiXe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .ThenInclude(pt => pt.LoaiPhuongTien)
                .GroupBy(p => p.PhuongTien.LoaiPhuongTien.TenLoaiPhuongTien)
                .Select(g => new ThongKeLoaiXe
                {
                    TenLoai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.5 Số lượt gửi theo từng vị trí
            viewModel.LuotGuiTheoViTri = await _context.PhieuGuiXe
                .Include(p => p.ViTriDoXe)
                .GroupBy(p => p.ViTriDoXe.TenViTri)
                .Select(g => new ThongKeLuotGuiViTri
                {
                    TenViTri = g.Key ?? "Không xác định",
                    SoLuot = g.Count()
                })
                .OrderByDescending(x => x.SoLuot)
                .ToListAsync();

            // 2.6 Vị trí có số lượt sử dụng nhiều nhất
            viewModel.ViTriDungNhieuNhat = viewModel.LuotGuiTheoViTri.FirstOrDefault();

            // 2.7 Số lượt gửi theo từng chủ phương tiện
            viewModel.LuotGuiTheoChuXe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .ThenInclude(pt => pt.ChuPhuongTien)
                .GroupBy(p => p.PhuongTien.ChuPhuongTien.HoTen)
                .Select(g => new ThongKeLuotGuiChuXe
                {
                    HoTen = g.Key ?? "Không xác định",
                    SoLuot = g.Count()
                })
                .OrderByDescending(x => x.SoLuot)
                .ToListAsync();

            // 2.8 Số lượt gửi và doanh thu theo các tháng thực tế có trong SQL (chỉ hiển thị từ tháng 1 đến tháng 10)
            int currentYear = DateTime.Now.Year;
            viewModel.LuotGuiVaDoanhThuTheoThang = await _context.PhieuGuiXe
                .Where(p => p.TrangThai == "Đã lấy xe" && p.ThanhTien != null && p.NgayDangKy.Year == currentYear)
                .GroupBy(p => p.NgayDangKy.Month)
                .Select(g => new ThongKeTheoThang
                {
                    Nam = currentYear,
                    Thang = g.Key,
                    SoLuot = g.Count(),
                    DoanhThu = g.Sum(p => p.ThanhTien) ?? 0
                })
                .OrderBy(x => x.Thang)
                .ToListAsync();

            // 2.9 Tính tỷ lệ lấp đầy theo từng tầng (dựa trên bảng ViTriDoXe)
            var danhSachViTri = await _context.ViTriDoXe.ToListAsync();
            viewModel.TyLeLapDauTheoTang = danhSachViTri
                .GroupBy(v => $"Tầng {v.Tang}") // Chuyển int thành chuỗi hiển thị
                .Select(g => new ThongKeTyLeTang
                {
                    TenTang = g.Key,
                    TongSoViTri = g.Count(),
                    SoViTriSuDung = g.Count(v => v.TrangThai == "Đang sử dụng"),
                    TyLe = g.Count() > 0 ? (int)Math.Round((double)g.Count(v => v.TrangThai == "Đang sử dụng") * 100.0 / g.Count()) : 0
                })
                .OrderBy(x => x.TenTang)
                .ToList();

            return View(viewModel);
        }
    }
}
