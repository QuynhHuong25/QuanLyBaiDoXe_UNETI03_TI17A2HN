using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class LoaiPhuongTiensController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public LoaiPhuongTiensController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: LOAIPHUONGTIENS
        public async Task<IActionResult> Index()
        {
            var listLoaiPhuongTien = _context.LoaiPhuongTien
                .Include(l => l.PhuongTiens)
                .Include(l => l.ViTriDoXes);

            return View(await listLoaiPhuongTien.ToListAsync());
        }

        // GET: LOAIPHUONGTIENS/Details/5
        public async Task<IActionResult> Details(int? maloaiphuongtien)
        {
            if (maloaiphuongtien == null)
            {
                return NotFound();
            }

            var loaiphuongtien = await _context.LoaiPhuongTien
                .Include(l => l.PhuongTiens)
                .Include(l => l.ViTriDoXes)
                .FirstOrDefaultAsync(m => m.MaLoaiPhuongTien == maloaiphuongtien);

            if (loaiphuongtien == null)
            {
                return NotFound();
            }

            return View(loaiphuongtien);
        }

        // GET: LOAIPHUONGTIENS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LOAIPHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLoaiPhuongTien,TenLoaiPhuongTien,DonGiaTheoGio,MoTa,TrangThai")] LoaiPhuongTien loaiphuongtien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiphuongtien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaiphuongtien);
        }

        // GET: LOAIPHUONGTIENS/Edit/5
        public async Task<IActionResult> Edit(int? maloaiphuongtien)
        {
            if (maloaiphuongtien == null)
            {
                return NotFound();
            }

            var loaiphuongtien = await _context.LoaiPhuongTien.FindAsync(maloaiphuongtien);
            if (loaiphuongtien == null)
            {
                return NotFound();
            }
            return View(loaiphuongtien);
        }

        // POST: LOAIPHUONGTIENS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? maloaiphuongtien, [Bind("MaLoaiPhuongTien,TenLoaiPhuongTien,DonGiaTheoGio,MoTa,TrangThai")] LoaiPhuongTien loaiphuongtien)
        {
            if (maloaiphuongtien != loaiphuongtien.MaLoaiPhuongTien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiphuongtien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiPhuongTienExists(loaiphuongtien.MaLoaiPhuongTien))
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
            return View(loaiphuongtien);
        }

        // GET: LOAIPHUONGTIENS/Delete/5
        public async Task<IActionResult> Delete(int? maloaiphuongtien)
        {
            if (maloaiphuongtien == null)
            {
                return NotFound();
            }

            var loaiphuongtien = await _context.LoaiPhuongTien
                .FirstOrDefaultAsync(m => m.MaLoaiPhuongTien == maloaiphuongtien);
            if (loaiphuongtien == null)
            {
                return NotFound();
            }

            return View(loaiphuongtien);
        }

        // POST: LOAIPHUONGTIENS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? maloaiphuongtien)
        {
            var loaiphuongtien = await _context.LoaiPhuongTien.FindAsync(maloaiphuongtien);
            if (loaiphuongtien != null)
            {
                _context.LoaiPhuongTien.Remove(loaiphuongtien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiPhuongTienExists(int? maloaiphuongtien)
        {
            return _context.LoaiPhuongTien.Any(e => e.MaLoaiPhuongTien == maloaiphuongtien);
        }
    }
}