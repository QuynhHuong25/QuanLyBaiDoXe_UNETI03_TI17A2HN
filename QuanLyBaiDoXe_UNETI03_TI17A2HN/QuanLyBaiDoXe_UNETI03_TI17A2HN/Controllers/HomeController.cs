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
            // Kiểm tra đăng nhập theo yêu cầu đề tài
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("TenDangNhap")))
            {
                return RedirectToAction("DangNhap", "TaiKhoans");
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            var queryPhieu = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .AsQueryable();

            // Phân quyền dữ liệu: Chỉ Quản trị viên (Admin) mới xem được toàn bộ hoạt động của bãi xe.
            // Khách hàng hoặc người dùng chưa đăng nhập chỉ xem được hoạt động của chính mình.
            if (vaiTro != "Admin")
            {
                if (!string.IsNullOrEmpty(maTaiKhoanStr) && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    var chuPT = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                    if (chuPT != null)
                    {
                        queryPhieu = queryPhieu.Where(p => p.PhuongTien != null && p.PhuongTien.MaChuPhuongTien == chuPT.MaChuPhuongTien);
                    }
                    else
                    {
                        queryPhieu = queryPhieu.Where(p => false);
                    }
                }
                else
                {
                    // Chưa đăng nhập hoặc không xác định được tài khoản -> không hiển thị dữ liệu gửi xe của người khác
                    queryPhieu = queryPhieu.Where(p => false);
                }
            }

            int soXeCuaToi = 0;
            if (vaiTro == "Admin")
            {
                soXeCuaToi = await _context.PhuongTien.CountAsync();
            }
            else if (int.TryParse(maTaiKhoanStr, out int mtk))
            {
                var chu = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == mtk);
                if (chu != null)
                    soXeCuaToi = await _context.PhuongTien.CountAsync(x => x.MaChuPhuongTien == chu.MaChuPhuongTien);
            }

            var viewModel = new HomeDashboardViewModel
            {
                // 5 lượt gửi gần đây (Admin: toàn bãi xe, Khách hàng: chỉ của mình)
                HoatDongGanDay = await queryPhieu
                    .OrderByDescending(p => p.NgayDangKy)
                    .Take(5)
                    .ToListAsync(),
                SoXeCuaToi = soXeCuaToi,
                SoLuotGui = await queryPhieu.CountAsync(),
                SoDangGui = await queryPhieu.CountAsync(p => p.TrangThai == "Đang gửi"),

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
        public async Task<IActionResult> GioiThieu()
        {
            ViewBag.SoDo = await _context.ViTriDoXe.OrderBy(v => v.TenViTri).Take(32).ToListAsync();
            ViewBag.TongViTri = await _context.ViTriDoXe.CountAsync();
            ViewBag.ViTriTrong = await _context.ViTriDoXe.CountAsync(v => v.TrangThai == "Còn trống");

            var bangGia = await _context.LoaiPhuongTien
                .Where(l => l.TrangThai).OrderBy(l => l.DonGiaTheoGio).ToListAsync();
            return View(bangGia);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
