using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using System.Text.RegularExpressions;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class ChuPhuongTiensController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        // Quy tắc kiểm tra dữ liệu cá nhân (có thể chỉnh tại đây)
        private const int TuoiToiThieu = 18;   // đổi thành 18 hoặc 0 nếu cần
        private const int TuoiToiDa = 100;
        private static readonly Regex RegexSdt = new(@"^0[35789]\d{8}$");
        private static readonly Regex RegexEmail = new(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$");

        public ChuPhuongTiensController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: CHUPHUONGTIENS
        public async Task<IActionResult> Index(int? page, string? searchMaTK, string? searchHoTen, string? searchSdt, string? locTrangThai)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var query = _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .AsQueryable();

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                // Khách hàng chỉ xem được thông tin chính mình
                if (int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    query = query.Where(c => c.MaTaiKhoan == maTaiKhoan);
                }
                else
                {
                    query = query.Where(c => false);
                }
            }
            else if (vaiTro == "Admin")
            {
                // Tìm theo mã tài khoản (khớp chính xác)
                if (!string.IsNullOrWhiteSpace(searchMaTK))
                {
                    if (int.TryParse(searchMaTK.Trim(), out int maTK))
                    {
                        query = query.Where(c => c.MaTaiKhoan == maTK);
                    }
                    else
                    {
                        query = query.Where(c => false);
                    }
                }

                // Tìm theo họ tên
                if (!string.IsNullOrWhiteSpace(searchHoTen))
                {
                    searchHoTen = searchHoTen.Trim();
                    query = query.Where(c => c.HoTen != null && c.HoTen.Contains(searchHoTen));
                }

                // Tìm theo số điện thoại
                if (!string.IsNullOrWhiteSpace(searchSdt))
                {
                    searchSdt = searchSdt.Trim();
                    query = query.Where(c => c.SoDienThoai != null && c.SoDienThoai.Contains(searchSdt));
                }

                // Lọc theo trạng thái
                if (locTrangThai == "hoatDong")
                {
                    query = query.Where(c => c.TrangThai);
                }
                else if (locTrangThai == "biKhoa")
                {
                    query = query.Where(c => !c.TrangThai);
                }
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var items = await query
                .OrderByDescending(c => c.MaChuPhuongTien)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.UserRole = vaiTro;
            ViewBag.Total = totalItems;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentMaTK = searchMaTK;
            ViewBag.CurrentHoTen = searchHoTen;
            ViewBag.CurrentSdt = searchSdt;
            ViewBag.CurrentTrangThai = locTrangThai;

            return View(items);
        }

        // GET: CHUPHUONGTIENS/Details/5
        public async Task<IActionResult> Details(int? machuphuongtien)
        {
            if (machuphuongtien == null)
            {
                return NotFound();
            }

            var chuphuongtien = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaChuPhuongTien == machuphuongtien);

            if (chuphuongtien == null)
            {
                return NotFound();
            }

            // Kiểm tra phân quyền truy cập thông tin
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (chuphuongtien.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Create
        [PhanQuyen("Admin")]
        public IActionResult Create()
        {
            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap");
            return View();
        }

        // POST: CHUPHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create([Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai,TaiKhoan,PhuongTiens")] ChuPhuongTien chuphuongtien)
        {
            // Tự động gán ngày đăng ký bằng thời gian hiện tại nếu chưa có
            if (chuphuongtien.NgayDangKy == null)
            {
                chuphuongtien.NgayDangKy = DateTime.Now;
            }

            // Kiểm tra xem Mã tài khoản này đã có chủ phương tiện nào sở hữu chưa
            bool daTonTaiTaiKhoan = await _context.ChuPhuongTien
                .AnyAsync(c => c.MaTaiKhoan == chuphuongtien.MaTaiKhoan);

            if (daTonTaiTaiKhoan)
            {
                ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với một chủ phương tiện khác trong hệ thống.");
            }

            // Kiểm tra email, số điện thoại, ngày sinh
            KiemTraThongTinCaNhan(chuphuongtien);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(chuphuongtien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm mới chủ phương tiện thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu dữ liệu: " + errorMsg);
                }
            }

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Edit/5
        public async Task<IActionResult> Edit(int? machuphuongtien)
        {
            if (machuphuongtien == null)
            {
                return NotFound();
            }

            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(machuphuongtien);
            if (chuphuongtien == null)
            {
                return NotFound();
            }

            // Khách hàng chỉ được chỉnh sửa bản ghi cá nhân
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (chuphuongtien.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
            return View(chuphuongtien);
        }

        // POST: CHUPHUONGTIENS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? machuphuongtien, [Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi")] ChuPhuongTien chuphuongtien)
        {
            if (machuphuongtien == null || machuphuongtien != chuphuongtien.MaChuPhuongTien)
            {
                return NotFound();
            }

            // Luôn lấy bản ghi gốc từ CSDL để kiểm tra quyền, không tin dữ liệu gửi từ form
            var chuCu = await _context.ChuPhuongTien
                .FirstOrDefaultAsync(c => c.MaChuPhuongTien == machuphuongtien);
            if (chuCu == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            bool isAdmin = vaiTro == "Admin";

            if (!isAdmin)
            {
                // Khách hàng chỉ được sửa hồ sơ của chính mình và không được đổi tài khoản liên kết
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan) || chuCu.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
                chuphuongtien.MaTaiKhoan = chuCu.MaTaiKhoan;
            }
            else
            {
                // Admin đổi tài khoản liên kết: tài khoản phải tồn tại và chưa gắn với chủ xe khác
                bool taiKhoanTonTai = await _context.TaiKhoan.AnyAsync(t => t.MaTaiKhoan == chuphuongtien.MaTaiKhoan);
                if (!taiKhoanTonTai)
                {
                    ModelState.AddModelError("MaTaiKhoan", "Tài khoản liên kết không tồn tại.");
                }
                else
                {
                    bool daCoChuKhac = await _context.ChuPhuongTien.AnyAsync(c =>
                        c.MaTaiKhoan == chuphuongtien.MaTaiKhoan && c.MaChuPhuongTien != chuCu.MaChuPhuongTien);
                    if (daCoChuKhac)
                    {
                        ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với một chủ phương tiện khác.");
                    }
                }
            }

            // Các giá trị không nằm trong form luôn lấy từ bản ghi gốc
            chuphuongtien.NgayDangKy = chuCu.NgayDangKy;
            chuphuongtien.TrangThai = chuCu.TrangThai;

            // Kiểm tra email, số điện thoại, ngày sinh
            KiemTraThongTinCaNhan(chuphuongtien);

            if (!ModelState.IsValid)
            {
                ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
                return View(chuphuongtien);
            }

            // Chỉ cập nhật các trường được phép sửa
            chuCu.MaTaiKhoan = chuphuongtien.MaTaiKhoan;
            chuCu.HoTen = chuphuongtien.HoTen?.Trim();
            chuCu.NgaySinh = chuphuongtien.NgaySinh;
            chuCu.GioiTinh = chuphuongtien.GioiTinh;
            chuCu.SoDienThoai = chuphuongtien.SoDienThoai;
            chuCu.Email = chuphuongtien.Email;
            chuCu.DiaChi = chuphuongtien.DiaChi?.Trim();

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChuPhuongTienExists(chuCu.MaChuPhuongTien))
                {
                    return NotFound();
                }
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> CapNhatTrangThai(
             int id,
             bool trangThai,
             string? searchMaTK,
             string? searchHoTen,
             string? searchSdt,
             string? locTrangThai,
             int page = 1)
        {
            var chuPhuongTien = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .FirstOrDefaultAsync(c => c.MaChuPhuongTien == id);

            if (chuPhuongTien == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái chủ phương tiện
            chuPhuongTien.TrangThai = trangThai;

            // Cập nhật trạng thái tài khoản đăng nhập
            if (chuPhuongTien.TaiKhoan != null)
            {
                chuPhuongTien.TaiKhoan.TrangThai = trangThai;
            }

            await _context.SaveChangesAsync();

            // Kiểm tra lại dữ liệu sau khi lưu
            var taiKhoanKiemTra = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == chuPhuongTien.MaTaiKhoan);

            if (taiKhoanKiemTra == null)
            {
                TempData["SuccessMessage"] =
                    "Đã cập nhật chủ phương tiện nhưng không tìm thấy tài khoản liên kết!";
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Đã {(trangThai ? "kích hoạt" : "khóa")} tài khoản thành công! " +
                    $"Trạng thái tài khoản: {taiKhoanKiemTra.TrangThai}";
            }

            return RedirectToAction(nameof(Index), new
            {
                page,
                searchMaTK,
                searchHoTen,
                searchSdt,
                locTrangThai
            });
        }

        // GET: CHUPHUONGTIENS/Delete/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Delete(int? machuphuongtien)
        {
            if (machuphuongtien == null)
            {
                return NotFound();
            }

            var chuphuongtien = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaChuPhuongTien == machuphuongtien);

            if (chuphuongtien == null)
            {
                return NotFound();
            }

            return View(chuphuongtien);
        }

        // POST: CHUPHUONGTIENS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? machuphuongtien)
        {
            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(machuphuongtien);
            if (chuphuongtien != null)
            {
                _context.ChuPhuongTien.Remove(chuphuongtien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa chủ phương tiện thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChuPhuongTienExists(int? machuphuongtien)
        {
            return _context.ChuPhuongTien.Any(e => e.MaChuPhuongTien == machuphuongtien);
        }

        private void KiemTraThongTinCaNhan(ChuPhuongTien model)
        {
           
            // ---------- Số điện thoại ----------
            ModelState.Remove(nameof(ChuPhuongTien.SoDienThoai)); // bỏ thông báo mặc định để dùng tiếng Việt
            model.SoDienThoai = model.SoDienThoai?.Trim();

            if (string.IsNullOrEmpty(model.SoDienThoai))
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.SoDienThoai), "Vui lòng nhập số điện thoại.");
            }
            else if (!RegexSdt.IsMatch(model.SoDienThoai))
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.SoDienThoai),
                    "Số điện thoại không hợp lệ (gồm 10 số, bắt đầu bằng 03, 05, 07, 08 hoặc 09).");
            }

            // ---------- Email ----------
            ModelState.Remove(nameof(ChuPhuongTien.Email));
            model.Email = model.Email?.Trim().ToLower();

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.Email), "Vui lòng nhập email.");
            }
            else if (model.Email.Length > 100)
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.Email), "Email tối đa 100 ký tự.");
            }
            else if (!RegexEmail.IsMatch(model.Email))
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.Email), "Email không đúng định dạng (ví dụ: ten@gmail.com).");
            }

            // ---------- Ngày sinh ----------
            ModelState.Remove(nameof(ChuPhuongTien.NgaySinh));

            if (model.NgaySinh == null)
            {
                ModelState.AddModelError(nameof(ChuPhuongTien.NgaySinh), "Vui lòng chọn ngày sinh hợp lệ.");
            }
            else
            {
                var ngaySinh = model.NgaySinh.Value.Date;
                var homNay = DateTime.Today;

                int tuoi = homNay.Year - ngaySinh.Year;
                if (ngaySinh > homNay.AddYears(-tuoi))
                {
                    tuoi--;
                }

                if (ngaySinh > homNay)
                {
                    ModelState.AddModelError(nameof(ChuPhuongTien.NgaySinh), "Ngày sinh không được ở tương lai.");
                }
                else if (tuoi > TuoiToiDa)
                {
                    ModelState.AddModelError(nameof(ChuPhuongTien.NgaySinh), "Ngày sinh không hợp lệ (quá lớn tuổi).");
                }
                else if (tuoi < TuoiToiThieu)
                {
                    ModelState.AddModelError(nameof(ChuPhuongTien.NgaySinh), $"Chủ phương tiện phải từ {TuoiToiThieu} tuổi trở lên.");
                }
            }
        }
    }
}
