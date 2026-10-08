using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class PhieuGuiXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public PhieuGuiXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: PHIEUGUIXES
        public async Task<IActionResult> Index(int? page, string? searchString, string? trangThaiFilter)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;
            ViewBag.TrangThaiFilter = trangThaiFilter;

            var query = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
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

            if (!string.IsNullOrWhiteSpace(trangThaiFilter))
            {
                query = query.Where(p => p.TrangThai == trangThaiFilter);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p => p.PhuongTien != null &&
                                         p.PhuongTien.BienSoXe != null &&
                                         p.PhuongTien.BienSoXe.Contains(searchString));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var danhSach = await query
                .OrderByDescending(p => p.NgayDangKy)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.UserRole = vaiTro;
            ViewBag.Total = totalItems;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(danhSach);
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
                .Where(v => v.TrangThai != "Tạm ngừng")
                .OrderBy(v => v.TenViTri)
                .ToListAsync();

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
            }

            bool trungLich = await _context.PhieuGuiXe.AnyAsync(p => p.MaViTri == phieuGuiXe.MaViTri &&
                                                                     p.TrangThai != "Đã hủy" &&
                                                                     p.TrangThai != "Đã lấy xe" &&
                                                                     p.ThoiGianDuKienVao < phieuGuiXe.ThoiGianDuKienRa &&
                                                                     p.ThoiGianDuKienRa > phieuGuiXe.ThoiGianDuKienVao);

            if (trungLich)
            {
                ModelState.AddModelError("MaViTri", "Vị trí này đã có phiếu gửi xe trùng khoảng thời gian.");
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
                .Where(v => v.TrangThai != "Tạm ngừng")
                .OrderBy(v => v.TenViTri)
                .ToListAsync();
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
                    _context.Update(phieuGuiXe);
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
            var phieuGuiXe = await _context.PhieuGuiXe.FindAsync(id);
            if (phieuGuiXe != null)
            {
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
    }
}