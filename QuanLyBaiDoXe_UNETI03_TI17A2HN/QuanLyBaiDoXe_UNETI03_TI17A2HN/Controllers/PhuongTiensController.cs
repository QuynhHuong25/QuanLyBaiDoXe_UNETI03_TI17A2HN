using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class PhuongTiensController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public PhuongTiensController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: PHUONGTIENS
        public async Task<IActionResult> Index(int? page, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            ViewBag.CurrentFilter = searchString;
            var query = _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
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
                    query = query.Where(p => p.MaChuPhuongTien == chuPhuongTien.MaChuPhuongTien);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(p => (p.BienSoXe != null && p.BienSoXe.Contains(searchString)) ||
                                         (p.NhanHieu != null && p.NhanHieu.Contains(searchString)));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var items = await query.OrderByDescending(p => p.MaPhuongTien)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listDangGui = await _context.PhieuGuiXe
                .Where(pg => pg.TrangThai == "Chờ xác nhận" || pg.TrangThai == "Đang gửi")
                .Select(pg => pg.MaPhuongTien)
                .Distinct()
                .ToListAsync();

            ViewBag.ListDangGui = listDangGui;
            ViewBag.UserRole = vaiTro;
            ViewBag.Total = totalItems;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        // GET: PHUONGTIENS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phuongTien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == id);

            if (phuongTien == null)
            {
                return NotFound();
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    return RedirectToAction(nameof(Index));
                }

                var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                if (chuPhuongTien == null || phuongTien.MaChuPhuongTien != chuPhuongTien.MaChuPhuongTien)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(phuongTien);
        }

        // GET: PHUONGTIENS/Create
        public async Task<IActionResult> Create()
        {
            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    TempData["ErrorMessage"] = "Không xác định được tài khoản đăng nhập.";
                    return RedirectToAction(nameof(Index));
                }

                var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                if (chuPhuongTien == null)
                {
                    TempData["ErrorMessage"] = "Tài khoản chưa được liên kết thông tin Chủ phương tiện.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.MaChuPhuongTien = new SelectList(new[] { chuPhuongTien }, "MaChuPhuongTien", "HoTen", chuPhuongTien.MaChuPhuongTien);
            }
            else
            {
                ViewBag.MaChuPhuongTien = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen");
            }

            ViewBag.MaLoaiPhuongTien = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien");
            return View();
        }

        // POST: PHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhuongTien,MaChuPhuongTien,MaLoaiPhuongTien,BienSoXe,NhanHieu,MauSac,MoTa,TrangThai")] PhuongTien phuongTien)
        {
            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (string.IsNullOrWhiteSpace(phuongTien.BienSoXe))
            {
                ModelState.AddModelError("BienSoXe", "Vui lòng nhập biển số xe.");
            }
            else
            {
                phuongTien.BienSoXe = phuongTien.BienSoXe.Trim().ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(phuongTien.BienSoXe))
            {
                bool trungBienSo = await _context.PhuongTien.AnyAsync(p => p.BienSoXe == phuongTien.BienSoXe);
                if (trungBienSo)
                {
                    ModelState.AddModelError("BienSoXe", "Biển số xe này đã tồn tại trong hệ thống.");
                }
            }

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    ModelState.AddModelError("", "Không xác định được tài khoản đăng nhập.");
                }
                else
                {
                    var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                    if (chuPhuongTien == null)
                    {
                        ModelState.AddModelError("", "Tài khoản chưa được liên kết thông tin Chủ phương tiện.");
                    }
                    else
                    {
                        phuongTien.MaChuPhuongTien = chuPhuongTien.MaChuPhuongTien;
                    }
                }
            }

            bool chuPhuongTienTonTai = await _context.ChuPhuongTien.AnyAsync(c => c.MaChuPhuongTien == phuongTien.MaChuPhuongTien);
            if (!chuPhuongTienTonTai)
            {
                ModelState.AddModelError("MaChuPhuongTien", "Chủ phương tiện không tồn tại.");
            }

            bool loaiPhuongTienTonTai = await _context.LoaiPhuongTien.AnyAsync(l => l.MaLoaiPhuongTien == phuongTien.MaLoaiPhuongTien);
            if (!loaiPhuongTienTonTai)
            {
                ModelState.AddModelError("MaLoaiPhuongTien", "Loại phương tiện không tồn tại.");
            }

            phuongTien.TrangThai = true;

            if (ModelState.IsValid)
            {
                _context.PhuongTien.Add(phuongTien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới phương tiện thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadCreateData(phuongTien, vaiTro, maTaiKhoanStr);
            return View(phuongTien);
        }

        // GET: PHUONGTIENS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phuongTien = await _context.PhuongTien.FirstOrDefaultAsync(p => p.MaPhuongTien == id);
            if (phuongTien == null)
            {
                return NotFound();
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    return RedirectToAction(nameof(Index));
                }

                var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                if (chuPhuongTien == null || phuongTien.MaChuPhuongTien != chuPhuongTien.MaChuPhuongTien)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.MaChuPhuongTien = new SelectList(new[] { chuPhuongTien }, "MaChuPhuongTien", "HoTen", phuongTien.MaChuPhuongTien);
            }
            else
            {
                ViewBag.MaChuPhuongTien = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongTien.MaChuPhuongTien);
            }

            ViewBag.MaLoaiPhuongTien = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongTien.MaLoaiPhuongTien);
            return View(phuongTien);
        }

        // POST: PHUONGTIENS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhuongTien,MaChuPhuongTien,MaLoaiPhuongTien,BienSoXe,NhanHieu,MauSac,MoTa,TrangThai")] PhuongTien phuongTien)
        {
            if (id != phuongTien.MaPhuongTien)
            {
                return NotFound();
            }

            var xeCu = await _context.PhuongTien.FirstOrDefaultAsync(p => p.MaPhuongTien == id);
            if (xeCu == null)
            {
                return NotFound();
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (string.IsNullOrWhiteSpace(phuongTien.BienSoXe))
            {
                ModelState.AddModelError("BienSoXe", "Vui lòng nhập biển số xe.");
            }
            else
            {
                phuongTien.BienSoXe = phuongTien.BienSoXe.Trim().ToUpper();
                bool trungBienSo = await _context.PhuongTien.AnyAsync(p => p.MaPhuongTien != id && p.BienSoXe == phuongTien.BienSoXe);
                if (trungBienSo)
                {
                    ModelState.AddModelError("BienSoXe", "Biển số xe này đã thuộc về phương tiện khác.");
                }
            }

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    ModelState.AddModelError("", "Không xác định được tài khoản đăng nhập.");
                }
                else
                {
                    var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                    if (chuPhuongTien == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy thông tin chủ phương tiện.");
                    }
                    else if (xeCu.MaChuPhuongTien != chuPhuongTien.MaChuPhuongTien)
                    {
                        return Forbid();
                    }
                    else
                    {
                        phuongTien.MaChuPhuongTien = xeCu.MaChuPhuongTien;
                    }
                }
            }

            bool chuPhuongTienTonTai = await _context.ChuPhuongTien.AnyAsync(c => c.MaChuPhuongTien == phuongTien.MaChuPhuongTien);
            if (!chuPhuongTienTonTai)
            {
                ModelState.AddModelError("MaChuPhuongTien", "Chủ phương tiện không tồn tại.");
            }

            bool loaiPhuongTienTonTai = await _context.LoaiPhuongTien.AnyAsync(l => l.MaLoaiPhuongTien == phuongTien.MaLoaiPhuongTien);
            if (!loaiPhuongTienTonTai)
            {
                ModelState.AddModelError("MaLoaiPhuongTien", "Loại phương tiện không tồn tại.");
            }

            bool dangGui = await _context.PhieuGuiXe.AnyAsync(p => p.MaPhuongTien == id && (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi"));
            if (dangGui && !phuongTien.TrangThai)
            {
                ModelState.AddModelError("TrangThai", "Không thể chuyển sang Ngừng hoạt động vì phương tiện đang gửi trong bãi.");
            }

            if (!ModelState.IsValid)
            {
                await LoadEditData(phuongTien, vaiTro, maTaiKhoanStr);
                return View(phuongTien);
            }

            xeCu.MaChuPhuongTien = phuongTien.MaChuPhuongTien;
            xeCu.MaLoaiPhuongTien = phuongTien.MaLoaiPhuongTien;
            xeCu.BienSoXe = phuongTien.BienSoXe;
            xeCu.NhanHieu = phuongTien.NhanHieu;
            xeCu.MauSac = phuongTien.MauSac;
            xeCu.MoTa = phuongTien.MoTa;
            xeCu.TrangThai = phuongTien.TrangThai;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật phương tiện thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhuongTienExists(phuongTien.MaPhuongTien))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // GET: PHUONGTIENS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phuongTien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == id);

            if (phuongTien == null)
            {
                return NotFound();
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    return RedirectToAction(nameof(Index));
                }

                var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                if (chuPhuongTien == null || phuongTien.MaChuPhuongTien != chuPhuongTien.MaChuPhuongTien)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(phuongTien);
        }

        // POST: PHUONGTIENS/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phuongTien = await _context.PhuongTien.FirstOrDefaultAsync(p => p.MaPhuongTien == id);
            if (phuongTien == null)
            {
                return RedirectToAction(nameof(Index));
            }

            string? vaiTro = HttpContext.Session.GetString("VaiTro");
            string? maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng")
            {
                if (!int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    return Forbid();
                }

                var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                if (chuPhuongTien == null || phuongTien.MaChuPhuongTien != chuPhuongTien.MaChuPhuongTien)
                {
                    return Forbid();
                }
            }

            bool dangGui = await _context.PhieuGuiXe.AnyAsync(p => p.MaPhuongTien == id && (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi"));
            if (dangGui)
            {
                TempData["ErrorMessage"] = "Không thể xóa phương tiện vì xe đang có lượt gửi xe hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            _context.PhuongTien.Remove(phuongTien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa phương tiện thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCreateData(PhuongTien phuongTien, string? vaiTro, string? maTaiKhoanStr)
        {
            if (vaiTro == "Khách hàng")
            {
                if (int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                    if (chuPhuongTien != null)
                    {
                        ViewBag.MaChuPhuongTien = new SelectList(new[] { chuPhuongTien }, "MaChuPhuongTien", "HoTen", chuPhuongTien.MaChuPhuongTien);
                    }
                    else
                    {
                        ViewBag.MaChuPhuongTien = new SelectList(Enumerable.Empty<ChuPhuongTien>(), "MaChuPhuongTien", "HoTen");
                    }
                }
                else
                {
                    ViewBag.MaChuPhuongTien = new SelectList(Enumerable.Empty<ChuPhuongTien>(), "MaChuPhuongTien", "HoTen");
                }
            }
            else
            {
                ViewBag.MaChuPhuongTien = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongTien.MaChuPhuongTien);
            }

            ViewBag.MaLoaiPhuongTien = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongTien.MaLoaiPhuongTien);
        }

        private async Task LoadEditData(PhuongTien phuongTien, string? vaiTro, string? maTaiKhoanStr)
        {
            if (vaiTro == "Khách hàng")
            {
                if (int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                {
                    var chuPhuongTien = await _context.ChuPhuongTien.FirstOrDefaultAsync(c => c.MaTaiKhoan == maTaiKhoan);
                    if (chuPhuongTien != null)
                    {
                        ViewBag.MaChuPhuongTien = new SelectList(new[] { chuPhuongTien }, "MaChuPhuongTien", "HoTen", phuongTien.MaChuPhuongTien);
                    }
                    else
                    {
                        ViewBag.MaChuPhuongTien = new SelectList(Enumerable.Empty<ChuPhuongTien>(), "MaChuPhuongTien", "HoTen");
                    }
                }
                else
                {
                    ViewBag.MaChuPhuongTien = new SelectList(Enumerable.Empty<ChuPhuongTien>(), "MaChuPhuongTien", "HoTen");
                }
            }
            else
            {
                ViewBag.MaChuPhuongTien = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongTien.MaChuPhuongTien);
            }

            ViewBag.MaLoaiPhuongTien = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongTien.MaLoaiPhuongTien);
        }

        private bool PhuongTienExists(int id)
        {
            return _context.PhuongTien.Any(p => p.MaPhuongTien == id);
        }
    }
}