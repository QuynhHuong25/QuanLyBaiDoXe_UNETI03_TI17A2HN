// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Điều hướng và xử lý dữ liệu cho Dashboard Trang chủ (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels;
using System.Diagnostics;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public HomeController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeDashboardViewModel
            {
                // 5 lượt gửi xe gần đây nhất
                HoatDongGanDay = await _context.PhieuGuiXe
                    .Include(p => p.PhuongTien)
                        .ThenInclude(pt => pt.LoaiPhuongTien)
                    .Include(p => p.ViTriDoXe)
                    .OrderByDescending(p => p.NgayDangKy)
                    .Take(5)
                    .ToListAsync(),

                // Danh sách loại xe phục vụ form đăng ký gửi xe nhanh
                DanhSachLoaiXe = await _context.LoaiPhuongTien
                    .OrderBy(l => l.TenLoaiPhuongTien)
                    .ToListAsync(),

                // Tổng hợp số liệu hiển thị nhanh
                TongXeDangGui = await _context.PhieuGuiXe.CountAsync(p => p.TrangThai == "Đang gửi"),
                TongViTriTrong = await _context.ViTriDoXe.CountAsync(v => v.TrangThai == "Còn trống"),
                TongKhachHang = await _context.ChuPhuongTien.CountAsync(),
                TongPhuongTien = await _context.PhuongTien.CountAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyNhanh(string bienSoXe, int? maLoaiPhuongTien, DateTime? thoiGianVao, string? ghiChu)
        {
            string? maTaiKhoanString = HttpContext.Session.GetString("MaTaiKhoan");
            if (string.IsNullOrEmpty(maTaiKhoanString))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện đăng ký gửi xe.";
                return RedirectToAction("DangNhap", "TaiKhoans");
            }

            if (string.IsNullOrWhiteSpace(bienSoXe))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập biển số xe.";
                return RedirectToAction(nameof(Index));
            }

            // Ghi nhận thông tin nhanh và điều hướng sang trang lập phiếu
            TempData["QuickBienSo"] = bienSoXe?.Trim().ToUpper();
            TempData["QuickMaLoai"] = maLoaiPhuongTien;
            TempData["QuickThoiGianVao"] = thoiGianVao?.ToString("yyyy-MM-ddTHH:mm");
            TempData["SuccessMessage"] = "Đã tiếp nhận thông tin xe. Vui lòng chọn vị trí đỗ và thời gian ra để hoàn tất đăng ký.";
            
            return RedirectToAction("Create", "PhieuGuiXes");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
