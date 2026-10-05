using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using System.Security.Claims;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
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

            ViewBag.CurrentFilter = searchString;

            var query = _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .AsQueryable();

            
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            if (role != "Admin" && !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int currentUserId))
            {
                query = query.Where(c => c.MaTaiKhoan == currentUserId);
            }

           
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.HoTen!.Contains(searchString) || c.SoDienThoai!.Contains(searchString));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }
        // GET: CHUPHUONGTIENS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var chuphuongtien = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaChuPhuongTien == id);

            if (chuphuongtien == null) return NotFound();

            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Create
        public IActionResult Create()
        {
            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap");
            return View();
        }

        // POST: CHUPHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai,TaiKhoan,PhuongTiens")] ChuPhuongTien chuPhuongTien)
        {
            
            if (chuPhuongTien.NgayDangKy == null)
            {
                chuPhuongTien.NgayDangKy = DateTime.Now;
            }

           
            bool daTonTaiTaiKhoan = await _context.ChuPhuongTien
                .AnyAsync(c => c.MaTaiKhoan == chuPhuongTien.MaTaiKhoan);

            if (daTonTaiTaiKhoan)
            {
                ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được liên kết với một chủ phương tiện khác trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(chuPhuongTien);
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

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuPhuongTien.MaTaiKhoan);
            return View(chuPhuongTien);
        }


        // GET: CHUPHUONGTIENS/Edit/5
        public async Task<IActionResult> Edit(int? id) 
        {
            if (id == null) return NotFound();

            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(id);
            if (chuphuongtien == null) return NotFound();

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
            return View(chuphuongtien);
        }

        // POST: CHUPHUONGTIENS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai")] ChuPhuongTien chuphuongtien)
        {
            if (id != chuphuongtien.MaChuPhuongTien) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuphuongtien);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MaTaiKhoan = new SelectList(_context.TaiKhoan, "MaTaiKhoan", "TenDangNhap", chuphuongtien.MaTaiKhoan);
            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var chuphuongtien = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaChuPhuongTien == id);

            if (chuphuongtien == null) return NotFound();

            return View(chuphuongtien);
        }

        // POST: CHUPHUONGTIENS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(id);
            if (chuphuongtien != null)
            {
                _context.ChuPhuongTien.Remove(chuphuongtien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuPhuongTienExists(int id)
        {
            return _context.ChuPhuongTien.Any(e => e.MaChuPhuongTien == id);
        }
    }
}
