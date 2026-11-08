// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Tạo Controller xử lý Thống kê và Dashboard (Module 4) - Đã tối ưu hiệu năng (AsNoTracking, SQL Aggregation)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

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
            int currentYear = DateTime.Now.Year;
            var viewModel = await LayDuLieuThongKeAsync(currentYear);
            return View(viewModel);
        }

        // ---- Chức năng nâng cao: Xuất Excel thống kê ----
        // Họ và tên: Tăng Hoàng Giang – MSV: 23103100031
        [HttpGet]
        public async Task<IActionResult> XuatExcel()
        {
            int currentYear = DateTime.Now.Year;
            var viewModel = await LayDuLieuThongKeAsync(currentYear);

            var service = new XuatBaoCaoService();
            byte[] bytes = service.ExcelThongKe(viewModel, currentYear);

            string tenFile = $"ThongKe_{currentYear}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenFile);
        }

        // Tối ưu hóa: Dùng chung hàm nạp dữ liệu thống kê, áp dụng AsNoTracking() để giảm tải ChangeTracker
        // và đẩy phép tính toán tổng hợp về SQL Server (Server-side GroupBy) thay vì kéo toàn bộ DB về RAM.
        private async Task<ThongKeViewModel> LayDuLieuThongKeAsync(int currentYear)
        {
            var viewModel = new ThongKeViewModel();

            // 1. Dashboard (Tổng quan) - AsNoTracking để tối ưu tài nguyên bộ nhớ
            viewModel.Dashboard.TongLoaiPhuongTien = await _context.LoaiPhuongTien.AsNoTracking().CountAsync();
            viewModel.Dashboard.TongViTriDo = await _context.ViTriDoXe.AsNoTracking().CountAsync();
            viewModel.Dashboard.TongChuPhuongTien = await _context.ChuPhuongTien.AsNoTracking().CountAsync();
            viewModel.Dashboard.TongPhuongTien = await _context.PhuongTien.AsNoTracking().CountAsync();
            viewModel.Dashboard.TongPhieuGuiXe = await _context.PhieuGuiXe.AsNoTracking().CountAsync();

            viewModel.Dashboard.SoPhieuChoXacNhan = await _context.PhieuGuiXe.AsNoTracking().CountAsync(p => p.TrangThai == "Chờ xác nhận");
            viewModel.Dashboard.SoXeDangGui = await _context.PhieuGuiXe.AsNoTracking().CountAsync(p => p.TrangThai == "Đang gửi");

            viewModel.Dashboard.SoViTriConTrong = await _context.ViTriDoXe.AsNoTracking().CountAsync(v => v.TrangThai == "Còn trống");
            viewModel.Dashboard.SoViTriDangSuDung = await _context.ViTriDoXe.AsNoTracking().CountAsync(v => v.TrangThai == "Đang sử dụng");

            // 2. Thống kê chi tiết bằng LINQ
            // 2.1 Số phương tiện theo từng loại (chỉ group trên cột TenLoaiPhuongTien, tránh Include thừa thãi)
            viewModel.SoPhuongTienTheoLoai = await _context.PhuongTien
                .AsNoTracking()
                .Where(p => p.LoaiPhuongTien != null)
                .GroupBy(p => p.LoaiPhuongTien!.TenLoaiPhuongTien)
                .Select(g => new ThongKeLoaiXe
                {
                    TenLoai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.2 Số vị trí theo từng trạng thái
            viewModel.SoViTriTheoTrangThai = await _context.ViTriDoXe
                .AsNoTracking()
                .GroupBy(v => v.TrangThai)
                .Select(g => new ThongKeTrangThai
                {
                    TrangThai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.3 Số phiếu theo từng trạng thái
            viewModel.SoPhieuTheoTrangThai = await _context.PhieuGuiXe
                .AsNoTracking()
                .GroupBy(p => p.TrangThai)
                .Select(g => new ThongKeTrangThai
                {
                    TrangThai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.4 Số lượt gửi theo từng loại phương tiện
            viewModel.LuotGuiTheoLoaiXe = await _context.PhieuGuiXe
                .AsNoTracking()
                .Where(p => p.PhuongTien != null && p.PhuongTien.LoaiPhuongTien != null)
                .GroupBy(p => p.PhuongTien!.LoaiPhuongTien!.TenLoaiPhuongTien)
                .Select(g => new ThongKeLoaiXe
                {
                    TenLoai = g.Key ?? "Không xác định",
                    SoLuong = g.Count()
                }).ToListAsync();

            // 2.5 Số lượt gửi theo từng vị trí
            viewModel.LuotGuiTheoViTri = await _context.PhieuGuiXe
                .AsNoTracking()
                .Where(p => p.ViTriDoXe != null)
                .GroupBy(p => p.ViTriDoXe!.TenViTri)
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
                .AsNoTracking()
                .Where(p => p.PhuongTien != null && p.PhuongTien.ChuPhuongTien != null)
                .GroupBy(p => p.PhuongTien!.ChuPhuongTien!.HoTen)
                .Select(g => new ThongKeLuotGuiChuXe
                {
                    HoTen = g.Key ?? "Không xác định",
                    SoLuot = g.Count()
                })
                .OrderByDescending(x => x.SoLuot)
                .ToListAsync();

            // 2.8 Số lượt gửi và doanh thu theo các tháng thực tế có trong SQL
            viewModel.LuotGuiVaDoanhThuTheoThang = await _context.PhieuGuiXe
                .AsNoTracking()
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

            // 2.9 Tính tỷ lệ lấp đầy theo từng tầng: Gom nhóm và tính số lượng trực tiếp tại SQL Server
            // Chỉ tải về đúng vài dòng kết quả (số tầng), thay vì kéo hàng ngàn vị trí đỗ về bộ nhớ RAM web.
            var thongKeTangDb = await _context.ViTriDoXe
                .AsNoTracking()
                .GroupBy(v => v.Tang)
                .Select(g => new
                {
                    Tang = g.Key,
                    TongSoViTri = g.Count(),
                    SoViTriSuDung = g.Count(v => v.TrangThai == "Đang sử dụng")
                })
                .OrderBy(x => x.Tang)
                .ToListAsync();

            viewModel.TyLeLapDauTheoTang = thongKeTangDb
                .Select(g => new ThongKeTyLeTang
                {
                    TenTang = $"Tầng {g.Tang}",
                    TongSoViTri = g.TongSoViTri,
                    SoViTriSuDung = g.SoViTriSuDung,
                    TyLe = g.TongSoViTri > 0 ? (int)Math.Round((double)g.SoViTriSuDung * 100.0 / g.TongSoViTri) : 0
                })
                .ToList();

            return viewModel;
        }
    }
}
