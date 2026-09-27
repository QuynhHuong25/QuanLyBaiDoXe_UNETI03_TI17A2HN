using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class PhieuGuiXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public PhieuGuiXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // GET: PHIEUGUIXES
        public async Task<IActionResult> Index()
        {
            var listPhieu = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe);

            return View(await listPhieu.ToListAsync());
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
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(m => m.MaPhieu == maphieu);

            if (phieuguixe == null)
            {
                return NotFound();
            }

            return View(phieuguixe);
        }

        // GET: PHIEUGUIXES/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PHIEUGUIXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,MaPhuongTien,MaViTri,NgayDangKy,ThoiGianDuKienVao,ThoiGianDuKienRa,ThoiGianVaoThucTe,ThoiGianRaThucTe,DonGiaTheoGio,ThanhTien,TrangThai")] PhieuGuiXe phieuguixe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuguixe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phieuguixe);
        }

        // GET: PHIEUGUIXES/Edit/5
        public async Task<IActionResult> Edit(int? maphieu)
        {
            if (maphieu == null)
            {
                return NotFound();
            }

            var phieuguixe = await _context.PhieuGuiXe.FindAsync(maphieu);
            if (phieuguixe == null)
            {
                return NotFound();
            }
            return View(phieuguixe);
        }

        // POST: PHIEUGUIXES/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? maphieu, [Bind("MaPhieu,MaPhuongTien,MaViTri,NgayDangKy,ThoiGianDuKienVao,ThoiGianDuKienRa,ThoiGianVaoThucTe,ThoiGianRaThucTe,DonGiaTheoGio,ThanhTien,TrangThai")] PhieuGuiXe phieuguixe)
        {
            if (maphieu != phieuguixe.MaPhieu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuguixe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuGuiXeExists(phieuguixe.MaPhieu))
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
            return View(phieuguixe);
        }

        // GET: PHIEUGUIXES/Delete/5
        public async Task<IActionResult> Delete(int? maphieu)
        {
            if (maphieu == null)
            {
                return NotFound();
            }

            var phieuguixe = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(m => m.MaPhieu == maphieu);

            if (phieuguixe == null)
            {
                return NotFound();
            }

            return View(phieuguixe);
        }

        // POST: PHIEUGUIXES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? maphieu)
        {
            var phieuguixe = await _context.PhieuGuiXe.FindAsync(maphieu);
            if (phieuguixe != null)
            {
                _context.PhieuGuiXe.Remove(phieuguixe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuGuiXeExists(int? maphieu)
        {
            return _context.PhieuGuiXe.Any(e => e.MaPhieu == maphieu);
        }
    }
}