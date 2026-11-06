using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    // Admin và Khách hàng được truy cập quản lý vé tháng
    [PhanQuyen("Admin", "Khách hàng")]
    public class VeThangsController : Controller
    {
        private static readonly int[] CacKyHan = { 1, 3, 6, 12 };

        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _email;
        private readonly XuatBaoCaoService _xuat;

        public VeThangsController(
            QuanLyBaiDoXe_UNETI03_TI17A2HNContext context,
            IConfiguration config,
            EmailService email,
            XuatBaoCaoService xuat)
        {
            _context = context;
            _config = config;
            _email = email;
            _xuat = xuat;
        }

        // Kiểm tra vai trò
        private bool LaAdmin => HttpContext.Session.GetString("VaiTro") == "Admin";
        private bool LaKhachHang => HttpContext.Session.GetString("VaiTro") == "Khách hàng";
        private bool LaAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";


        // =====================================================
        // LẤY MÃ CHỦ PHƯƠNG TIỆN ĐANG ĐĂNG NHẬP
        // =====================================================

        private async Task<int?> MaChuHienTaiAsync()
        {
            if (!int.TryParse(HttpContext.Session.GetString("MaTaiKhoan"), out int maTk))
            {
                return null;
            }

            return await _context.ChuPhuongTien
                .Where(c => c.MaTaiKhoan == maTk)
                .Select(c => (int?)c.MaChuPhuongTien)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // TÍNH TIỀN VÉ THÁNG
        // =====================================================

        private static decimal TyLeGiam(int soThang)
        {
            return soThang switch
            {
                3 => 0.05m,
                6 => 0.10m,
                12 => 0.15m,
                _ => 0m
            };
        }

        private decimal TinhTien(decimal donGiaGio, int soThang)
        {
            int gioMoiThang = _config.GetValue("VeThang:SoGioMoiThang", 200);
            decimal goc = donGiaGio * gioMoiThang * soThang;
            return Math.Round(goc * (1 - TyLeGiam(soThang)), 0);
        }


        // =====================================================
        // TRUY VẤN DANH SÁCH VÉ THÁNG
        // =====================================================

        private async Task<IQueryable<VeThang>> TaoTruyVanAsync(string? searchBienSo, string? filterTrangThai)
        {
            var query = _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.ChuPhuongTien)
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.LoaiPhuongTien)
                .AsQueryable();

            // Khách hàng chỉ xem vé của xe mình
            if (!LaAdmin)
            {
                int? maChu = await MaChuHienTaiAsync();
                query = maChu == null
                    ? query.Where(v => false)
                    : query.Where(v => v.PhuongTien != null && v.PhuongTien.MaChuPhuongTien == maChu);
            }

            // Tìm kiếm theo biển số
            if (!string.IsNullOrWhiteSpace(searchBienSo))
            {
                var kw = searchBienSo.Trim();
                query = query.Where(v => v.PhuongTien != null && v.PhuongTien.BienSoXe != null && v.PhuongTien.BienSoXe.Contains(kw));
            }

            var homNay = DateTime.Today;

            // Lọc trạng thái vé
            query = filterTrangThai switch
            {
                "choThanhToan" => query.Where(v => v.TrangThai == "Chờ thanh toán"),
                "dangHieuLuc" => query.Where(v => v.TrangThai == "Hoạt động" && v.NgayBatDau <= homNay && v.NgayKetThuc >= homNay),
                "chuaBatDau" => query.Where(v => v.TrangThai == "Hoạt động" && v.NgayBatDau > homNay),
                "hetHan" => query.Where(v => v.TrangThai == "Hoạt động" && v.NgayKetThuc < homNay),
                "daHuy" => query.Where(v => v.TrangThai == "Đã hủy"),
                _ => query
            };

            return query.OrderByDescending(v => v.NgayDangKy);
        }


        // =====================================================
        // DANH SÁCH VÉ THÁNG
        // =====================================================

        // GET: VeThangs
        public async Task<IActionResult> Index(string? searchBienSo, string? filterTrangThai, int page = 1)
        {
            var query = await TaoTruyVanAsync(searchBienSo, filterTrangThai);

            int pageSize = 10;
            int tong = await query.CountAsync();
            int tongTrang = Math.Max(1, (int)Math.Ceiling(tong / (double)pageSize));

            page = Math.Max(1, Math.Min(page, tongTrang));

            var ds = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = tongTrang;
            ViewBag.TotalItems = tong;
            ViewBag.SearchBienSo = searchBienSo;
            ViewBag.FilterTrangThai = filterTrangThai;
            ViewBag.LaAdmin = LaAdmin;

            return View(ds);
        }


        // =====================================================
        // ĐĂNG KÝ VÉ THÁNG - CHỈ KHÁCH HÀNG
        // =====================================================

        // GET: VeThangs/Create
        public async Task<IActionResult> Create()
        {
            // Admin không được đăng ký vé tháng
            if (!LaKhachHang) return Forbid();

            await NapDanhSachXeAsync();

            return View(new VeThang
            {
                NgayBatDau = DateTime.Today,
                SoThang = 1
            });
        }


        // POST: VeThangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhuongTien,NgayBatDau,SoThang,GhiChu")] VeThang ve)
        {
            // Chỉ khách hàng được đăng ký vé tháng
            if (!LaKhachHang) return Forbid();

            // Lấy thông tin phương tiện
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
                // Kiểm tra phương tiện có hoạt động không
                if (!xe.TrangThai)
                {
                    ModelState.AddModelError("MaPhuongTien", "Phương tiện này đang ngừng hoạt động.");
                }

                // Kiểm tra xe có thuộc khách hàng đang đăng nhập không
                int? maChu = await MaChuHienTaiAsync();
                if (maChu == null || xe.MaChuPhuongTien != maChu)
                {
                    ModelState.AddModelError("MaPhuongTien", "Bạn chỉ được đăng ký vé tháng cho xe của mình.");
                }
            }

            // Kiểm tra kỳ hạn
            if (!CacKyHan.Contains(ve.SoThang))
            {
                ModelState.AddModelError("SoThang", "Kỳ hạn không hợp lệ (chỉ 1, 3, 6 hoặc 12 tháng).");
            }

            // Kiểm tra ngày bắt đầu
            ve.NgayBatDau = ve.NgayBatDau.Date;
            if (ve.NgayBatDau < DateTime.Today)
            {
                ModelState.AddModelError("NgayBatDau", "Ngày bắt đầu không được ở trong quá khứ.");
            }

            // Tính ngày kết thúc
            ve.NgayKetThuc = ve.NgayBatDau.AddMonths(Math.Max(1, ve.SoThang)).AddDays(-1);

            // Kiểm tra vé bị trùng thời gian
            if (xe != null && await CoVeTrungKhoangAsync(xe.MaPhuongTien, ve.NgayBatDau, ve.NgayKetThuc, null))
            {
                ModelState.AddModelError("NgayBatDau", "Xe này đã có vé tháng trùng khoảng thời gian đã chọn.");
            }

            // Hai trường này do hệ thống tự xử lý
            ModelState.Remove(nameof(VeThang.TrangThai));
            ModelState.Remove(nameof(VeThang.NgayKetThuc));

            // Nếu dữ liệu không hợp lệ thì quay lại form
            if (!ModelState.IsValid || xe == null)
            {
                await NapDanhSachXeAsync();
                return View(ve);
            }

            // Tính tiền vé
            ve.GiaTien = TinhTien(xe.LoaiPhuongTien?.DonGiaTheoGio ?? 0, ve.SoThang);

            // Thiết lập trạng thái ban đầu
            ve.TrangThai = "Chờ thanh toán";
            ve.NgayDangKy = DateTime.Now;

            _context.VeThang.Add(ve);
            await _context.SaveChangesAsync();

            // Chuyển sang trang thanh toán QR
            return RedirectToAction(nameof(ThanhToan), new { id = ve.MaVeThang });
        }


        // =====================================================
        // THANH TOÁN VÉ THÁNG
        // =====================================================

        // GET: VeThangs/ThanhToan/5
        [HttpGet]
        public async Task<IActionResult> ThanhToan(int id)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.LoaiPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve)) return Forbid();

            if (ve.TrangThai != "Chờ thanh toán")
            {
                TempData[ve.TrangThai == "Hoạt động" ? "SuccessMessage" : "ErrorMessage"] =
                    ve.TrangThai == "Hoạt động"
                        ? $"Vé tháng #{ve.MaVeThang} đã được thanh toán."
                        : "Vé này đã bị hủy.";

                return RedirectToAction(nameof(Index));
            }

            var cfg = _config.GetSection("ThanhToan");
            string nganHang = cfg["MaNganHang"] ?? "BIDV";
            string stk = cfg["SoTaiKhoan"] ?? "";
            string ten = cfg["TenTaiKhoan"] ?? "";
            string noiDung = $"VETHANG {ve.MaVeThang}";

            // Tạo mã QR VietQR
            ViewBag.QrUrl = $"https://img.vietqr.io/image/{nganHang}-{stk}-compact2.png" +
                            $"?amount={(long)ve.GiaTien}" +
                            $"&addInfo={Uri.EscapeDataString(noiDung)}" +
                            $"&accountName={Uri.EscapeDataString(ten)}";

            ViewBag.NganHang = cfg["TenNganHang"] ?? nganHang;
            ViewBag.SoTaiKhoan = stk;
            ViewBag.TenTaiKhoan = ten;
            ViewBag.NoiDung = noiDung;
            ViewBag.LaAdmin = LaAdmin;
            ViewBag.TuDong = _config.GetValue("ThanhToan:TuDongXacNhan", false);

            return View(ve);
        }


        // =====================================================
        // HOÀN TẤT THANH TOÁN
        // =====================================================

        // POST: VeThangs/HoanTatThanhToan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanTatThanhToan(int id)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.ChuPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve)) return Forbid();

            if (ve.TrangThai != "Chờ thanh toán")
            {
                TempData["ErrorMessage"] = "Vé này không ở trạng thái chờ thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không bật chế độ tự động xác nhận
            if (!_config.GetValue("ThanhToan:TuDongXacNhan", false))
            {
                TempData["SuccessMessage"] = $"Đã ghi nhận thanh toán vé #{ve.MaVeThang}. " +
                                             "Vé sẽ được kích hoạt sau khi quản trị viên xác nhận đã nhận tiền.";
                return RedirectToAction(nameof(Index));
            }

            // Kích hoạt vé sau khi thanh toán
            ve.TrangThai = "Hoạt động";
            await _context.SaveChangesAsync();

            var chu = ve.PhuongTien?.ChuPhuongTien;

            // Gửi email xác nhận
            _ = _email.GuiXacNhanVeThangAsync(
                chu?.Email,
                chu?.HoTen ?? "",
                ve.PhuongTien?.BienSoXe ?? "",
                ve);

            TempData["ThoiGianTT"] = DateTime.Now.ToString("dd/MM/yyyy, HH:mm:ss");

            return RedirectToAction(nameof(ThanhCong), new { id = ve.MaVeThang });
        }


        // =====================================================
        // THANH TOÁN THÀNH CÔNG
        // =====================================================

        // GET: VeThangs/ThanhCong/5
        [HttpGet]
        public async Task<IActionResult> ThanhCong(int id)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.LoaiPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve)) return Forbid();
            if (ve.TrangThai != "Hoạt động") return RedirectToAction(nameof(Index));

            ViewBag.ThoiGian = TempData["ThoiGianTT"] as string ?? ve.NgayDangKy.ToString("dd/MM/yyyy, HH:mm:ss");
            ViewBag.MaGiaoDich = $"#{ve.NgayDangKy:yyMMdd}{ve.MaVeThang:D6}";
            ViewBag.NoiDung = $"VETHANG {ve.MaVeThang}";

            return View(ve);
        }


        // =====================================================
        // ADMIN XÁC NHẬN THANH TOÁN
        // =====================================================

        // POST: VeThangs/XacNhanThanhToan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.ChuPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();

            if (ve.TrangThai != "Chờ thanh toán")
            {
                TempData["ErrorMessage"] = "Vé này không ở trạng thái chờ thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Kích hoạt vé
            ve.TrangThai = "Hoạt động";
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            var chu = ve.PhuongTien?.ChuPhuongTien;

            _ = _email.GuiXacNhanVeThangAsync(
                chu?.Email,
                chu?.HoTen ?? "",
                ve.PhuongTien?.BienSoXe ?? "",
                ve);

            TempData["SuccessMessage"] = $"Đã xác nhận thanh toán và kích hoạt vé tháng #{ve.MaVeThang}.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // GIA HẠN VÉ THÁNG
        // =====================================================

        // POST: VeThangs/GiaHan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id, int soThang)
        {
            var ve = await _context.VeThang
                .Include(v => v.PhuongTien)
                    .ThenInclude(p => p!.LoaiPhuongTien)
                .FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();
            if (!await DuocQuyenAsync(ve)) return Forbid();

            // Kiểm tra kỳ hạn
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

            if (ve.TrangThai == "Chờ thanh toán")
            {
                TempData["ErrorMessage"] = "Vé đang chờ thanh toán. Hãy thanh toán vé trước khi gia hạn.";
                return RedirectToAction(nameof(Index));
            }

            DateTime batDauMoi = ve.NgayBatDau;
            DateTime ketThucMoi;

            bool daHetHan = ve.NgayKetThuc.Date < DateTime.Today;

            if (daHetHan)
            {
                // Vé hết hạn: tính từ hôm nay
                batDauMoi = DateTime.Today;
                ketThucMoi = batDauMoi.AddMonths(soThang).AddDays(-1);
            }
            else
            {
                // Vé còn hạn: nối tiếp ngày kết thúc hiện tại
                ketThucMoi = ve.NgayKetThuc.Date.AddDays(1).AddMonths(soThang).AddDays(-1);
            }

            // Kiểm tra trùng thời gian
            if (await CoVeTrungKhoangAsync(ve.MaPhuongTien, batDauMoi, ketThucMoi, ve.MaVeThang))
            {
                TempData["ErrorMessage"] = "Không thể gia hạn vì trùng với một vé tháng khác của xe này.";
                return RedirectToAction(nameof(Index));
            }

            // Tính phí gia hạn
            decimal them = TinhTien(ve.PhuongTien?.LoaiPhuongTien?.DonGiaTheoGio ?? 0, soThang);

            ve.NgayBatDau = batDauMoi;
            ve.NgayKetThuc = ketThucMoi;
            ve.SoThang = daHetHan ? soThang : ve.SoThang + soThang;
            ve.GiaTien = daHetHan ? them : ve.GiaTien + them;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã gia hạn vé tháng #{ve.MaVeThang} đến {ve.NgayKetThuc:dd/MM/yyyy} (phí thêm {them:N0} đ).";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // HỦY VÉ THÁNG
        // =====================================================

        // POST: VeThangs/Huy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var ve = await _context.VeThang.FirstOrDefaultAsync(v => v.MaVeThang == id);

            if (ve == null) return NotFound();

            if (!await DuocQuyenAsync(ve))
            {
                return LaAjax
                    ? Json(new { ok = false, message = "Bạn không có quyền hủy vé này." })
                    : Forbid();
            }

            string? loi = null;

            if (ve.TrangThai == "Đã hủy")
            {
                loi = "Vé này đã được hủy trước đó.";
            }
            else if (ve.NgayKetThuc.Date < DateTime.Today)
            {
                loi = "Vé đã hết hạn, không cần hủy.";
            }

            if (loi == null)
            {
                ve.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
            }

            if (LaAjax)
            {
                return Json(new { ok = loi == null, message = loi ?? "Đã hủy vé tháng." });
            }

            TempData[loi == null ? "SuccessMessage" : "ErrorMessage"] = loi ?? "Đã hủy vé tháng.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // TÍNH GIÁ VÉ THÁNG BẰNG AJAX
        // CHỈ KHÁCH HÀNG ĐƯỢC SỬ DỤNG
        // =====================================================

        // GET: VeThangs/TinhGia
        [HttpGet]
        public async Task<IActionResult> TinhGia(int maPhuongTien, int soThang, DateTime? ngayBatDau)
        {
            // Chỉ khách hàng được xem báo giá đăng ký
            if (!LaKhachHang)
            {
                return Json(new { ok = false, message = "Bạn không có quyền thực hiện chức năng này." });
            }

            if (!CacKyHan.Contains(soThang))
            {
                return Json(new { ok = false, message = "Kỳ hạn không hợp lệ." });
            }

            var xe = await _context.PhuongTien
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == maPhuongTien);

            if (xe == null)
            {
                return Json(new { ok = false, message = "Chưa chọn phương tiện." });
            }

            // Kiểm tra quyền sở hữu xe
            int? maChu = await MaChuHienTaiAsync();
            if (maChu == null || xe.MaChuPhuongTien != maChu)
            {
                return Json(new { ok = false, message = "Không có quyền." });
            }

            if (!xe.TrangThai)
            {
                return Json(new { ok = false, message = "Phương tiện đang ngừng hoạt động." });
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


        // =====================================================
        // ADMIN XUẤT EXCEL
        // =====================================================

        // GET: VeThangs/XuatExcel
        [PhanQuyen("Admin")]
        public async Task<IActionResult> XuatExcel(string? searchBienSo, string? filterTrangThai)
        {
            var ds = await (await TaoTruyVanAsync(searchBienSo, filterTrangThai)).ToListAsync();
            var bytes = _xuat.ExcelVeThang(ds);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ve-thang-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
        }


        // =====================================================
        // CÁC HÀM HỖ TRỢ
        // =====================================================

        // Lấy danh sách phương tiện
        private async Task NapDanhSachXeAsync()
        {
            var q = _context.PhuongTien
                .Include(p => p.LoaiPhuongTien)
                .Where(p => p.TrangThai);

            // Khách hàng chỉ lấy xe của mình
            if (!LaAdmin)
            {
                int? maChu = await MaChuHienTaiAsync();
                q = maChu == null ? q.Where(p => false) : q.Where(p => p.MaChuPhuongTien == maChu);
            }

            ViewBag.DanhSachXe = await q
                .OrderBy(p => p.BienSoXe)
                .ToListAsync();
        }


        // Kiểm tra quyền sở hữu vé tháng
        private async Task<bool> DuocQuyenAsync(VeThang ve)
        {
            // Admin được quyền quản lý tất cả vé
            if (LaAdmin) return true;

            // Khách hàng chỉ được thao tác vé của mình
            int? maChu = await MaChuHienTaiAsync();
            if (maChu == null) return false;

            return await _context.PhuongTien.AnyAsync(p =>
                p.MaPhuongTien == ve.MaPhuongTien &&
                p.MaChuPhuongTien == maChu);
        }


        // Kiểm tra hai vé có bị trùng thời gian không
        private Task<bool> CoVeTrungKhoangAsync(int maPhuongTien, DateTime batDau, DateTime ketThuc, int? boQua)
        {
            return _context.VeThang.AnyAsync(v =>
                v.MaPhuongTien == maPhuongTien &&
                v.TrangThai != "Đã hủy" &&
                v.NgayBatDau <= ketThuc &&
                v.NgayKetThuc >= batDau &&
                (boQua == null || v.MaVeThang != boQua));
        }
    }
}
