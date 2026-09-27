using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

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
        public async Task<IActionResult> Index()
        {
            var list = await _context.ChuPhuongTien
                .Include(c => c.TaiKhoan)
                .ToListAsync();
            return View(list);
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

            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CHUPHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChuPhuongTien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai,TaiKhoan,PhuongTiens")] ChuPhuongTien chuphuongtien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chuphuongtien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(chuphuongtien);
        }

        // GET: CHUPHUONGTIENS/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int? machuphuongtien)
        {
            var chuphuongtien = await _context.ChuPhuongTien.FindAsync(machuphuongtien);
            if (chuphuongtien != null)
            {
                _context.ChuPhuongTien.Remove(chuphuongtien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuPhuongTienExists(int? machuphuongtien)
        {
            return _context.ChuPhuongTien.Any(e => e.MaChuPhuongTien == machuphuongtien);
        }
    }
}