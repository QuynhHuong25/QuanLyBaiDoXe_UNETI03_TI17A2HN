// Họ và tên: Trịnh Thị Hay (MSV: 23103100112) - Module 3: Đăng ký gửi xe (Create, Details, Edit)
// Họ và tên: Tăng Hoàng Giang (MSV: 23103100031) - Module 4: Quản lý danh sách phiếu (Index, Tìm kiếm, Lọc), Hủy phiếu an toàn

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class PhieuGuiXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;
        private readonly QrVeService _qr;

        public PhieuGuiXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context, QrVeService qr)
        {
            _context = context;
            _qr = qr;
        }

        // GET: PHIEUGUIXES
        public async Task<IActionResult> Index(
            string? searchBienSo,
            string? searchTenChuXe,
            string? filterTrangThai,
            string? filterLoaiXe,
            DateTime? filterNgayDangKy,
            string? searchString,
            string? trangThaiFilter,
            int page = 1)
        {
            // Hỗ trợ cả tên tham số mới và cũ
            searchBienSo = searchBienSo ?? searchString;
            filterTrangThai = filterTrangThai ?? trangThaiFilter;

            ViewData["searchBienSo"] = searchBienSo;
            ViewData["searchTenChuXe"] = searchTenChuXe;
            ViewData["filterTrangThai"] = filterTrangThai;
            ViewData["filterLoaiXe"] = filterLoaiXe;
            ViewData["filterNgayDangKy"] = filterNgayDangKy?.ToString("yyyy-MM-dd");

            ViewBag.LoaiXeList = await _context.LoaiPhuongTien.ToListAsync();

            var query = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.ChuPhuongTien)
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .AsQueryable();

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    TempData["ErrorMessage"] = "Không xác định được tài khoản đăng nhập.";
                    return RedirectToAction("Index", "Home");
                }

                var chuPhuongTien = await _context.ChuPhuongTien
                    .FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);

                if (chuPhuongTien == null)
                {
                    query = query.Where(p => false);
                }
                else
                {
                    query = query.Where(p => p.PhuongTien != null &&
                                             p.PhuongTien.MaChuPhuongTien == chuPhuongTien.MaChuPhuongTien);
                }
            }

            if (!string.IsNullOrWhiteSpace(filterTrangThai))
            {
                query = query.Where(p => p.TrangThai == filterTrangThai);
            }

            if (!string.IsNullOrWhiteSpace(searchBienSo))
            {
                query = query.Where(p => p.PhuongTien != null &&
                                         p.PhuongTien.BienSoXe != null &&
                                         p.PhuongTien.BienSoXe.Contains(searchBienSo));
            }

            if (!string.IsNullOrWhiteSpace(searchTenChuXe))
            {
                query = query.Where(p => p.PhuongTien != null &&
                                         p.PhuongTien.ChuPhuongTien != null &&
                                         p.PhuongTien.ChuPhuongTien.HoTen.Contains(searchTenChuXe));
            }

            if (!string.IsNullOrWhiteSpace(filterLoaiXe))
            {
                query = query.Where(p => p.PhuongTien != null &&
                                         p.PhuongTien.LoaiPhuongTien != null &&
                                         p.PhuongTien.LoaiPhuongTien.TenLoaiPhuongTien == filterLoaiXe);
            }

            if (filterNgayDangKy.HasValue)
            {
                query = query.Where(p => p.NgayDangKy.Date == filterNgayDangKy.Value.Date);
            }

            query = query.OrderByDescending(p => p.NgayDangKy);

            // Phân trang danh sách phiếu gửi xe (10 bản ghi / trang)
            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(items);
        }

        // GET: PHIEUGUIXES/Details/5
        public async Task<IActionResult> Details(int? maphieu)
        {
            if (maphieu == null)
            {
                return NotFound();
            }

            var phieuguixe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .ThenInclude(pt => pt.ChuPhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(m => m.MaPhieu == maphieu);

            if (phieuguixe == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (phieuguixe.PhuongTien?.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(phieuguixe);
        }

        // GET: PHIEUGUIXES/Ve/5 – Vé gửi xe kèm mã QR
        [PhanQuyen("Khách hàng")]
        public async Task<IActionResult> Ve(int? maphieu)
        {
            if (maphieu == null)
            {
                return NotFound();
            }

            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt!.ChuPhuongTien)
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt!.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == maphieu);

            if (phieu == null)
            {
                return NotFound();
            }

            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan) ||
                phieu.PhuongTien?.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaQr = _qr.Tao(phieu.MaPhieu);
            ViewBag.ConHieuLuc = phieu.TrangThai == "Chờ xác nhận" || phieu.TrangThai == "Đang gửi";
            return View(phieu);
        }

        // GET: PHIEUGUIXES/Create
        public async Task<IActionResult> Create()
        {
            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanString = HttpContext.Session.GetString("MaTaiKhoan");

            if (string.IsNullOrEmpty(maTaiKhoanString) || !int.TryParse(maTaiKhoanString, out int maTaiKhoan))
            {
                return RedirectToAction("DangNhap", "TaiKhoans");
            }

            List<PhuongTien> danhSachXe;

            if (vaiTro == "Khách hàng")
            {
                var chuPT = await _context.ChuPhuongTien
                    .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

                if (chuPT == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin chủ phương tiện.";
                    return RedirectToAction("Index", "Home");
                }

                danhSachXe = await _context.PhuongTien
                    .Include(p => p.LoaiPhuongTien)
                    .Where(p => p.MaChuPhuongTien == chuPT.MaChuPhuongTien && p.TrangThai == true)
                    .OrderBy(p => p.BienSoXe)
                    .ToListAsync();
            }
            else
            {
                danhSachXe = await _context.PhuongTien
                    .Include(p => p.LoaiPhuongTien)
                    .Where(p => p.TrangThai == true)
                    .OrderBy(p => p.BienSoXe)
                    .ToListAsync();
            }

            ViewBag.DanhSachXe = danhSachXe;

            ViewBag.DanhSachViTri = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .OrderBy(v => v.Tang).ThenBy(v => v.KhuVuc).ThenBy(v => v.TenViTri)
                .ToListAsync();
            ViewBag.PhieuGiuCho = await LayPhieuGiuChoAsync();

            return View();
        }

        // POST: PHIEUGUIXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuGuiXe phieuGuiXe)
        {
            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanString = HttpContext.Session.GetString("MaTaiKhoan");

            if (string.IsNullOrEmpty(maTaiKhoanString) || !int.TryParse(maTaiKhoanString, out int maTaiKhoan))
            {
                return RedirectToAction("DangNhap", "TaiKhoans");
            }

            if (phieuGuiXe.ThoiGianDuKienRa <= phieuGuiXe.ThoiGianDuKienVao)
            {
                ModelState.AddModelError("ThoiGianDuKienRa", "Thời gian dự kiến ra phải sau thời gian dự kiến vào.");
            }

            var phuongTien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == phieuGuiXe.MaPhuongTien);

            if (phuongTien == null)
            {
                ModelState.AddModelError("MaPhuongTien", "Phương tiện không tồn tại.");
            }
            else
            {
                if (!phuongTien.TrangThai)
                {
                    ModelState.AddModelError("MaPhuongTien", "Phương tiện này đang ngừng hoạt động.");
                }

                if (vaiTro == "Khách hàng")
                {
                    var chuPT = await _context.ChuPhuongTien
                        .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

                    if (chuPT == null || phuongTien.MaChuPhuongTien != chuPT.MaChuPhuongTien)
                    {
                        ModelState.AddModelError("MaPhuongTien", "Bạn chỉ được đăng ký xe thuộc tài khoản của mình.");
                    }
                }
            }

            bool xeDangGui = await _context.PhieuGuiXe.AnyAsync(p => p.MaPhuongTien == phieuGuiXe.MaPhuongTien &&
                                                                   (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi"));

            if (xeDangGui)
            {
                ModelState.AddModelError("MaPhuongTien", "Phương tiện này đang có một lượt gửi xe hoạt động.");
            }

            var viTri = await _context.ViTriDoXe.FirstOrDefaultAsync(v => v.MaViTri == phieuGuiXe.MaViTri);

            if (viTri == null)
            {
                ModelState.AddModelError("MaViTri", "Vị trí đỗ xe không tồn tại.");
            }
            else
            {
                if (viTri.TrangThai == "Tạm ngừng")
                {
                    ModelState.AddModelError("MaViTri", "Vị trí này đang tạm ngừng.");
                }

                if (phuongTien != null && phuongTien.MaLoaiPhuongTien != viTri.MaLoaiPhuongTien)
                {
                    ModelState.AddModelError("MaViTri", "Vị trí đỗ không phù hợp với loại phương tiện.");
                }

                if (await ViTriDangCoXeAsync(viTri, phieuGuiXe.ThoiGianDuKienVao))
                {
                    ModelState.AddModelError(string.Empty, "Vị trí đỗ này hiện đang có xe khác sử dụng!");
                }
                else if (await CoPhieuTrungKhoangThoiGianAsync(viTri.MaViTri, phieuGuiXe.ThoiGianDuKienVao, phieuGuiXe.ThoiGianDuKienRa))
                {
                    ModelState.AddModelError("MaViTri", "Vị trí này đã có phiếu gửi xe trùng khoảng thời gian. Vui lòng chọn vị trí hoặc thời gian khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateData(vaiTro, maTaiKhoan, phieuGuiXe);
                return View(phieuGuiXe);
            }

            phieuGuiXe.NgayDangKy = DateTime.Now;

            if (phuongTien == null || phuongTien.LoaiPhuongTien == null)
            {
                ModelState.AddModelError("MaPhuongTien", "Phương tiện chưa có loại phương tiện.");
                await LoadCreateData(vaiTro, maTaiKhoan, phieuGuiXe);
                return View(phieuGuiXe);
            }

            phieuGuiXe.DonGiaTheoGio = phuongTien.LoaiPhuongTien.DonGiaTheoGio;
            phieuGuiXe.TrangThai = "Chờ xác nhận";
            phieuGuiXe.ThoiGianVaoThucTe = null;
            phieuGuiXe.ThoiGianRaThucTe = null;
            phieuGuiXe.ThanhTien = 0;

            _context.PhieuGuiXe.Add(phieuGuiXe);
            await _context.SaveChangesAsync();

            if (vaiTro == "Khách hàng")
            {
                TempData["SuccessMessage"] = "Đăng ký gửi xe thành công. Vui lòng đưa mã QR bên dưới cho nhân viên quét khi vào và ra bãi.";
                return RedirectToAction(nameof(Ve), new { maphieu = phieuGuiXe.MaPhieu });
            }

            TempData["SuccessMessage"] = "Đăng ký gửi xe thành công. Phiếu đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCreateData(string? vaiTro, int maTaiKhoan, PhieuGuiXe? phieuGuiXe = null)
        {
            List<PhuongTien> danhSachXe;

            if (vaiTro == "Khách hàng")
            {
                var chuPT = await _context.ChuPhuongTien.FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
                if (chuPT != null)
                {
                    danhSachXe = await _context.PhuongTien
                        .Include(p => p.LoaiPhuongTien)
                        .Where(p => p.MaChuPhuongTien == chuPT.MaChuPhuongTien && p.TrangThai == true)
                        .OrderBy(p => p.BienSoXe)
                        .ToListAsync();
                }
                else
                {
                    danhSachXe = new List<PhuongTien>();
                }
            }
            else
            {
                danhSachXe = await _context.PhuongTien
                    .Include(p => p.LoaiPhuongTien)
                    .Where(p => p.TrangThai == true)
                    .OrderBy(p => p.BienSoXe)
                    .ToListAsync();
            }

            ViewBag.DanhSachXe = danhSachXe;
            ViewBag.DanhSachViTri = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .OrderBy(v => v.Tang).ThenBy(v => v.KhuVuc).ThenBy(v => v.TenViTri)
                .ToListAsync();
            ViewBag.PhieuGiuCho = await LayPhieuGiuChoAsync();
        }

        private async Task<List<object>> LayPhieuGiuChoAsync()
        {
            var ds = await _context.PhieuGuiXe
                .Where(p => p.MaViTri != null && (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi"))
                .Select(p => new { p.MaViTri, p.ThoiGianDuKienVao, p.ThoiGianDuKienRa, p.TrangThai })
                .ToListAsync();

            return ds.Select(p => (object)new
            {
                v = p.MaViTri,
                a = p.ThoiGianDuKienVao.ToString("yyyy-MM-ddTHH:mm:ss"),
                b = p.ThoiGianDuKienRa.ToString("yyyy-MM-ddTHH:mm:ss"),
                g = p.TrangThai == "Đang gửi"
            }).ToList();
        }

        private async Task<bool> ViTriDangCoXeAsync(ViTriDoXe viTri, DateTime vao)
        {
            if (viTri.TrangThai != "Đang sử dụng")
            {
                return false;
            }

            var gioRaDuKien = await _context.PhieuGuiXe
                .Where(p => p.MaViTri == viTri.MaViTri && p.TrangThai == "Đang gửi")
                .Select(p => (DateTime?)p.ThoiGianDuKienRa)
                .MaxAsync();

            var bayGio = DateTime.Now;
            var chiemDen = gioRaDuKien.HasValue && gioRaDuKien.Value > bayGio
                ? gioRaDuKien.Value
                : bayGio;

            return vao < chiemDen;
        }

        private async Task<bool> CoPhieuTrungKhoangThoiGianAsync(int maViTri, DateTime vao, DateTime ra, int? boQuaMaPhieu = null)
        {
            return await _context.PhieuGuiXe.AnyAsync(p =>
                p.MaViTri == maViTri &&
                (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi") &&
                p.ThoiGianDuKienVao < ra &&
                p.ThoiGianDuKienRa > vao &&
                (boQuaMaPhieu == null || p.MaPhieu != boQuaMaPhieu));
        }

        // GET: PHIEUGUIXES/Edit/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? id, int? maphieu)
        {
            var targetId = id ?? maphieu;
            if (targetId == null)
            {
                return NotFound();
            }

            var phieuGuiXe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(m => m.MaPhieu == targetId);

            if (phieuGuiXe == null)
            {
                return NotFound();
            }

            ViewData["MaPhuongTien"] = new SelectList(_context.PhuongTien, "MaPhuongTien", "BienSoXe", phieuGuiXe.MaPhuongTien);
            ViewData["MaViTri"] = new SelectList(_context.ViTriDoXe, "MaViTri", "TenViTri", phieuGuiXe.MaViTri);
            return View(phieuGuiXe);
        }

        // POST: PHIEUGUIXES/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhieu,MaPhuongTien,MaViTri,NgayDangKy,ThoiGianDuKienVao,ThoiGianDuKienRa,ThoiGianVaoThucTe,ThoiGianRaThucTe,DonGiaTheoGio,ThanhTien,TrangThai")] PhieuGuiXe phieuGuiXe)
        {
            if (id != phieuGuiXe.MaPhieu)
            {
                return NotFound();
            }

            if (phieuGuiXe.ThoiGianDuKienRa <= phieuGuiXe.ThoiGianDuKienVao)
            {
                ModelState.AddModelError("ThoiGianDuKienRa", "Thời gian dự kiến ra phải sau thời gian dự kiến vào.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPhieu = await _context.PhieuGuiXe.FindAsync(id);
                    if (existingPhieu == null)
                    {
                        return NotFound();
                    }

                    existingPhieu.MaPhuongTien = phieuGuiXe.MaPhuongTien;
                    existingPhieu.MaViTri = phieuGuiXe.MaViTri;
                    existingPhieu.ThoiGianDuKienVao = phieuGuiXe.ThoiGianDuKienVao;
                    existingPhieu.ThoiGianDuKienRa = phieuGuiXe.ThoiGianDuKienRa;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật phiếu gửi xe thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuGuiXeExists(phieuGuiXe.MaPhieu))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.MaPhuongTien = new SelectList(_context.PhuongTien, "MaPhuongTien", "BienSoXe", phieuGuiXe.MaPhuongTien);
            ViewBag.MaViTri = new SelectList(_context.ViTriDoXe, "MaViTri", "TenViTri", phieuGuiXe.MaViTri);

            return View(phieuGuiXe);
        }

        // GET: PHIEUGUIXES/Delete/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuGuiXe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieuGuiXe == null)
            {
                return NotFound();
            }

            return View(phieuGuiXe);
        }

        // POST: PHIEUGUIXES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuGuiXe = await _context.PhieuGuiXe
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieuGuiXe != null)
            {
                // Nếu xóa phiếu đang gửi, giải phóng ô đỗ
                if (phieuGuiXe.TrangThai == "Đang gửi" && phieuGuiXe.ViTriDoXe != null && phieuGuiXe.ViTriDoXe.TrangThai != "Tạm ngừng")
                {
                    phieuGuiXe.ViTriDoXe.TrangThai = "Còn trống";
                }

                _context.PhieuGuiXe.Remove(phieuGuiXe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa phiếu gửi xe thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PhieuGuiXeExists(int id)
        {
            return _context.PhieuGuiXe.Any(e => e.MaPhieu == id);
        }

        // HỦY PHIẾU (Module 4)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu(int id)
        {
            var phieu = await _context.PhieuGuiXe
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                var phuongTien = await _context.PhuongTien
                    .Include(pt => pt.ChuPhuongTien)
                    .FirstOrDefaultAsync(pt => pt.MaPhuongTien == phieu.MaPhuongTien);

                if (phuongTien?.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return Unauthorized();
                }
            }

            if (phieu.TrangThai != "Chờ xác nhận")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy phiếu đang ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction(nameof(Index));
            }

            // Thực hiện đổi trạng thái thành Đã hủy
            phieu.TrangThai = "Đã hủy";

            // Nếu vị trí đỗ bị ràng buộc bởi phiếu này, đảm bảo giữ vị trí ở trạng thái Còn trống (nếu không Tạm ngừng)
            if (phieu.ViTriDoXe != null && phieu.ViTriDoXe.TrangThai != "Tạm ngừng")
            {
                bool xeKhacDangDo = await _context.PhieuGuiXe
                    .AnyAsync(p => p.MaViTri == phieu.MaViTri && p.MaPhieu != phieu.MaPhieu && p.TrangThai == "Đang gửi");

                phieu.ViTriDoXe.TrangThai = xeKhacDangDo ? "Đang sử dụng" : "Còn trống";
            }

            _context.Update(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy phiếu thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
