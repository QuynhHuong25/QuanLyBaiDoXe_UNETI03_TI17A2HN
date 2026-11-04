// Chức năng nâng cao: Quản lý vé tháng
// - Khách hàng: đăng ký, xem, gia hạn, hủy vé tháng của xe mình
// - Admin: xem tất cả, đăng ký hộ, gia hạn, hủy, xuất Excel
// Xe có vé tháng còn hạn được miễn phí khi ra bãi (xử lý trong VaoRaController)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class VeThangsController : Controller
    {
        private static readonly int[] CacKyHan = { 1, 3, 6, 12 };

        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _email;
        private readonly XuatBaoCaoService _xuat;

        public VeThangsController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context, IConfiguration config,
                                  EmailService email, XuatBaoCaoService xuat)
        {
            _context = context;
            _config = config;
            _email = email;
            _xuat = xuat;
        }

        private bool LaAdmin => HttpContext.Session.GetString("VaiTro") == "Admin";
        private bool LaAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        // Mã chủ phương tiện của khách đang đăng nhập (null nếu không xác định)
        private async Task<int?> MaChuHienTaiAsync()
        {
            if (!int.TryParse(HttpContext.Session.GetString("MaTaiKhoan"), out int maTk)) return null;
            return await _context.ChuPhuongTien
                .Where(c => c.MaTaiKhoan == maTk)
                .Select(c => (int?)c.MaChuPhuongTien)
                .FirstOrDefaultAsync();
        }

        // ---------- Tính tiền ----------
        private static decimal TyLeGiam(int soThang) => soThang switch { 3 => 0.05m, 6 => 0.10m, 12 => 0.15m, _ => 0m };

        private decimal TinhTien(decimal donGiaGio, int soThang)
        {
            int gioMoiThang = _config.GetValue("VeThang:SoGioMoiThang", 200);
            decimal goc = donGiaGio * gioMoiThang * soThang;
            return Math.Round(goc * (1 - TyLeGiam(soThang)), 0);
        }

        // ---------- Truy vấn dùng chung (có phân quyền + bộ lọc) ----------
        private async Task<IQueryable<VeThang>> TaoTruyVanAsync(string? searchBienSo, string? filterTrangThai)
        {
            var query = _context.VeThang
                .Include(v => v.PhuongTien).ThenInclude(p => p!.ChuPhuongTien)
                .Include(v => v.PhuongTien).ThenInclude(p => p!.LoaiPhuongTien)
                .AsQueryable();

            if (!LaAdmin)
            {
                int? maChu = await MaChuHienTaiAsync();
                query = maChu == null
                    ? query.Where(v => false)
                    : query.Where(v => v.PhuongTien != null && v.PhuongTien.MaChuPhuongTien == maChu);
            }

            if (!string.IsNullOrWhiteSpace(searchBienSo))
            {
                var kw = searchBienSo.Trim();
                query = query.Where(v => v.PhuongTien != null && v.PhuongTien.BienSoXe != null && v.PhuongTien.BienSoXe.Contains(kw));
            }

            var homNay = DateTime.Today;
            query = filterTrangThai switch
            {
                "dangHieuLuc" => query.Where(v => v.TrangThai != "Đã hủy" && v.NgayBatDau <= homNay && v.NgayKetThuc >= homNay),
                "chuaBatDau" => query.Where(v => v.TrangThai != "Đã hủy" && v.NgayBatDau > homNay),
                "hetHan" => query.Where(v => v.TrangThai != "Đã hủy" && v.NgayKetThuc < homNay),
                "daHuy" => query.Where(v => v.TrangThai == "Đã hủy"),
                _ => query
            };

            return query.OrderByDescending(v => v.NgayDangKy);
        }

        // GET: VeThangs
        public async Task<IActionResult> Index(string? searchBienSo, string? filterTrangThai, int page = 1)
        {
            var query = await TaoTruyVanAsync(searchBienSo, filterTrangThai);

            int pageSize = 10;
            int tong = await query.CountAsync();
            int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)pageSize));
            page = Math.Max(1, Math.Min(page, tongTrang));

            var ds = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongTrang;
            ViewBag.TotalItems = tong;
            ViewBag.SearchBienSo = searchBienSo;
            ViewBag.FilterTrangThai = filterTrangThai;
            ViewBag.LaAdmin = LaAdmin;
            return View(ds);
        }

        // GET: VeThangs/Create
        public async Task<IActionResult> Create()
        {
            await NapDanhSachXeAsync();
            return View(new VeThang { NgayBatDau = DateTime.Today, SoThang = 1 });
        }

        // POST: VeThangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhuongTien,NgayBatDau,SoThang,GhiChu")] VeThang ve)
        {
            var xe = await _context.PhuongTien
                .Include(p => p.LoaiPhuongTien)
                .Include(p => p.ChuPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == ve.MaPhuongTien);

            if (xe == null)
            {
                ModelState.AddModelError("MaPhuongTien", "Phương tiện không tồn tại.");
            }
            else
            {
                if (!xe.TrangThai)
                    ModelState.AddModelError("MaPhuongTien", "Phương tiện này đang ngừng hoạt động.");

                if (!LaAdmin)
                {
                    int? maChu = await MaChuHienTaiAsync();
                    if (maChu == null || xe.MaChuPhuongTien != maChu)
                        ModelState.AddModelError("MaPhuongTien", "Bạn chỉ được đăng ký vé tháng cho xe của mình.");
                }
            }

            if (!CacKyHan.Contains(ve.SoThang))
                ModelState.AddModelError("SoThang", "Kỳ hạn không hợp lệ (chỉ 1, 3, 6 hoặc 12 tháng).");

            ve.NgayBatDau = ve.NgayBatDau.Date;
            if (ve.NgayBatDau < DateTime.Today)
                ModelState.AddModelError("NgayBatDau", "Ngày bắt đầu không được ở trong quá khứ.");

            ve.NgayKetThuc = ve.NgayBatDau.AddMonths(Math.Max(1, ve.SoThang)).AddDays(-1);

            if (xe != null && await CoVeTrungKhoangAsync(xe.MaPhuongTien, ve.NgayBatDau, ve.NgayKetThuc, null))
                ModelState.AddModelError("NgayBatDau", "Xe này đã có vé tháng trùng khoảng thời gian đã chọn.");

            // Các trường không do người dùng nhập
            ModelState.Remove(nameof(VeThang.TrangThai));
            ModelState.Remove(nameof(VeThang.NgayKetThuc));

            if (!ModelState.IsValid || xe == null)
            {
                await NapDanhSachXeAsync();
                return View(ve);
            }

            ve.GiaTien = TinhTien(xe.LoaiPhuongTien?.DonGiaTheoGio ?? 0, ve.SoThang);
            ve.TrangThai = "Hoạt động";
            ve.NgayDangKy = DateTime.Now;

            _context.VeThang.Add(ve);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: VeThangs/GiaHan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id, int soThang)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien).ThenInclude(p => p!.LoaiPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);
            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve)) return Forbid();

            if (!CacKyHan.Contains(soThang))
            {
                TempData["ErrorMessage"] = "Kỳ hạn gia hạn không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }
            if (ve.TrangThai == "Đã hủy")
            {
                TempData["ErrorMessage"] = "Vé đã hủy, không thể gia hạn. Hãy đăng ký vé mới.";
                return RedirectToAction(nameof(Index));
            }

            DateTime batDauMoi = ve.NgayBatDau, ketThucMoi;
            bool daHetHan = ve.NgayKetThuc.Date < DateTime.Today;
            if (daHetHan)
            {
                // Vé đã hết hạn: tính lại từ hôm nay
                batDauMoi = DateTime.Today;
                ketThucMoi = batDauMoi.AddMonths(soThang).AddDays(-1);
            }
            else
            {
                // Vé còn hạn: nối tiếp sau ngày kết thúc hiện tại
                ketThucMoi = ve.NgayKetThuc.Date.AddDays(1).AddMonths(soThang).AddDays(-1);
            }

            if (await CoVeTrungKhoangAsync(ve.MaPhuongTien, batDauMoi, ketThucMoi, ve.MaVeThang))
            {
                TempData["ErrorMessage"] = "Không thể gia hạn vì trùng với một vé tháng khác của xe này.";
                return RedirectToAction(nameof(Index));
            }

            decimal them = TinhTien(ve.PhuongTien?.LoaiPhuongTien?.DonGiaTheoGio ?? 0, soThang);
            ve.NgayBatDau = batDauMoi;
            ve.NgayKetThuc = ketThucMoi;
            ve.SoThang = daHetHan ? soThang : ve.SoThang + soThang;
            ve.GiaTien = daHetHan ? them : ve.GiaTien + them;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã gia hạn vé tháng #{ve.MaVeThang} đến {ve.NgayKetThuc:dd/MM/yyyy} (phí thêm {them:N0} đ).";
            return RedirectToAction(nameof(Index));
        }

        // POST: VeThangs/Huy  (hỗ trợ AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var ve = await _context.VeThang.FirstOrDefaultAsync(v => v.MaVeThang == id);
            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve))
                return LaAjax ? Json(new { ok = false, message = "Bạn không có quyền hủy vé này." }) : Forbid();

            string? loi = null;
            if (ve.TrangThai == "Đã hủy") loi = "Vé này đã được hủy trước đó.";
            else if (ve.NgayKetThuc.Date < DateTime.Today) loi = "Vé đã hết hạn, không cần hủy.";

            if (loi == null)
            {
                ve.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
            }

            if (LaAjax) return Json(new { ok = loi == null, message = loi ?? "Đã hủy vé tháng." });

            TempData[loi == null ? "SuccessMessage" : "ErrorMessage"] = loi ?? "Đã hủy vé tháng.";
            return RedirectToAction(nameof(Index));
        }

        // GET: VeThangs/TinhGia?maPhuongTien=1&soThang=3  (AJAX xem trước giá)
        [HttpGet]
        public async Task<IActionResult> TinhGia(int maPhuongTien, int soThang, DateTime? ngayBatDau)
        {
            if (!CacKyHan.Contains(soThang)) return Json(new { ok = false, message = "Kỳ hạn không hợp lệ." });

            var xe = await _context.PhuongTien.Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == maPhuongTien);
            if (xe == null) return Json(new { ok = false, message = "Chưa chọn phương tiện." });

            if (!LaAdmin)
            {
                int? maChu = await MaChuHienTaiAsync();
                if (maChu == null || xe.MaChuPhuongTien != maChu) return Json(new { ok = false, message = "Không có quyền." });
            }

            decimal donGia = xe.LoaiPhuongTien?.DonGiaTheoGio ?? 0;
            int gioMoiThang = _config.GetValue("VeThang:SoGioMoiThang", 200);
            decimal goc = donGia * gioMoiThang * soThang;
            decimal tong = TinhTien(donGia, soThang);
            var bd = (ngayBatDau ?? DateTime.Today).Date;

            return Json(new
            {
                ok = true,
                loaiXe = xe.LoaiPhuongTien?.TenLoaiPhuongTien,
                donGia = donGia.ToString("N0"),
                gioMoiThang,
                gocText = goc.ToString("N0"),
                giamPhanTram = (int)(TyLeGiam(soThang) * 100),
                tongText = tong.ToString("N0"),
                ketThuc = bd.AddMonths(soThang).AddDays(-1).ToString("dd/MM/yyyy")
            });
        }

        // GET: VeThangs/XuatExcel
        [PhanQuyen("Admin")]
        public async Task<IActionResult> XuatExcel(string? searchBienSo, string? filterTrangThai)
        {
            var ds = await (await TaoTruyVanAsync(searchBienSo, filterTrangThai)).ToListAsync();
            var bytes = _xuat.ExcelVeThang(ds);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ve-thang-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }

        // ---------- Hàm hỗ trợ ----------
        private async Task NapDanhSachXeAsync()
        {
            var q = _context.PhuongTien.Include(p => p.LoaiPhuongTien).Where(p => p.TrangThai);
            if (!LaAdmin)
            {
                int? maChu = await MaChuHienTaiAsync();
                q = maChu == null ? q.Where(p => false) : q.Where(p => p.MaChuPhuongTien == maChu);
            }
            ViewBag.DanhSachXe = await q.OrderBy(p => p.BienSoXe).ToListAsync();
        }

        // Khách chỉ thao tác được trên vé của xe mình
        private async Task<bool> DuocQuyenAsync(VeThang ve)
        {
            if (LaAdmin) return true;
            int? maChu = await MaChuHienTaiAsync();
            if (maChu == null) return false;
            return await _context.PhuongTien.AnyAsync(p => p.MaPhuongTien == ve.MaPhuongTien && p.MaChuPhuongTien == maChu);
        }

        // Hai vé (chưa hủy) của cùng một xe không được chồng lấn thời gian
        private Task<bool> CoVeTrungKhoangAsync(int maPhuongTien, DateTime batDau, DateTime ketThuc, int? boQua)
        {
            return _context.VeThang.AnyAsync(v =>
                v.MaPhuongTien == maPhuongTien &&
                v.TrangThai != "Đã hủy" &&
                v.NgayBatDau <= ketThuc && v.NgayKetThuc >= batDau &&
                (boQua == null || v.MaVeThang != boQua));
        }
    }
}
