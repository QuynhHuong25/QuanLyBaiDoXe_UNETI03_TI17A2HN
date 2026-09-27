using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class ViTriDoXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public ViTriDoXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: VITRIDOXES
        public async Task<IActionResult> Index()
        {
            var listViTriDoXe = _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien);

            return View(await listViTriDoXe.ToListAsync());
        }

        // GET: VITRIDOXES/Details/5
        public async Task<IActionResult> Details(int? mavitri)
        {
            if (mavitri == null)
            {
                return NotFound();
            }

            var vitridoxe = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .FirstOrDefaultAsync(m => m.MaViTri == mavitri);

            if (vitridoxe == null)
            {
                return NotFound();
            }

            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VITRIDOXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vitridoxe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Edit/5
        public async Task<IActionResult> Edit(int? mavitri)
        {
            if (mavitri == null)
            {
                return NotFound();
            }

            var vitridoxe = await _context.ViTriDoXe.FindAsync(mavitri);
            if (vitridoxe == null)
            {
                return NotFound();
            }
            return View(vitridoxe);
        }

        // POST: VITRIDOXES/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? mavitri, [Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (mavitri != vitridoxe.MaViTri)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vitridoxe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViTriDoXeExists(vitridoxe.MaViTri))
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
            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Delete/5
        public async Task<IActionResult> Delete(int? mavitri)
        {
            if (mavitri == null)
            {
                return NotFound();
            }

            var vitridoxe = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .FirstOrDefaultAsync(m => m.MaViTri == mavitri);

            if (vitridoxe == null)
            {
                return NotFound();
            }

            return View(vitridoxe);
        }

        // POST: VITRIDOXES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? mavitri)
        {
            var vitridoxe = await _context.ViTriDoXe.FindAsync(mavitri);
            if (vitridoxe != null)
            {
                _context.ViTriDoXe.Remove(vitridoxe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViTriDoXeExists(int? mavitri)
        {
            return _context.ViTriDoXe.Any(e => e.MaViTri == mavitri);
        }
    }
}