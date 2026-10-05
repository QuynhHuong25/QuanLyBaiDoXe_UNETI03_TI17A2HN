using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class ChuPhuongTiensController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public ChuPhuongTiensController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: CHUPHUONGTIENS
        public async Task<IActionResult> Index(int? page, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .AsQueryable();

           
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                // Khách hàng chỉ xem được thông tin chính mình
                query = query.Where(c => c.MaTaiKhoan == maTaiKhoan);
            }

          
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c =>
                    (c.HoTen != null && c.HoTen.Contains(searchString)) ||
                    (c.SoDienThoai != null && c.SoDienThoai.Contains(searchString))
                );
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(c => c.MaChuPhuongTien)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.UserRole = vaiTro;
            ViewBag.Total = totalItems;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentFilter = searchString;

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
        public async Task<IActionResult> Edit(int? machuphuongtien, [Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai,TaiKhoan,PhuongTiens")] ChuPhuongTien chuphuongtien)
        {
            if (machuphuongtien != chuphuongtien.MaChuPhuongTien)
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuphuongtien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuPhuongTienExists(chuphuongtien.MaChuPhuongTien))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
            return View(chuphuongtien);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> CapNhatTrangThai(int id, bool trangThai, string? searchString, int page = 1)
        {
            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(id);
            if (chuphuongtien == null)
            {
                return NotFound();
            }

            chuphuongtien.TrangThai = trangThai;
            _context.Update(chuphuongtien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = trangThai ? "Đã kích hoạt chủ phương tiện thành công!" : "Đã khóa chủ phương tiện thành công!";

            // Giữ nguyên trang và từ khóa tìm kiếm khi chuyển hướng về Index
            return RedirectToAction(nameof(Index), new { page = page, searchString = searchString });
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
    }
}
