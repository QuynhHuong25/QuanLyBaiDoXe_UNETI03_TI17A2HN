using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using System.Net.Http.Headers;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class ViTriDoXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public ViTriDoXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }
        //Kiem tra quyen Admin tu Session
        private bool IsAdmin()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            return !string.IsNullOrEmpty(vaiTro) && vaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
        // GET: VITRIDOXES
        public async Task<IActionResult> Index(string searchString)
        {///tim kiem
            ViewData["CurrentSearch"] = searchString;
            var query = _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.TenViTri.Contains(searchString) || v.KhuVuc.Contains(searchString));
            }

            return View(await query.ToListAsync());
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

        // GET: VITRIDOXES/Create (Admin moi duoc vao)
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien");
            return View();
        }

        // POST: VITRIDOXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if(!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
            //ten vi tri khong duoc trung
            if (await _context.ViTriDoXe.AnyAsync(v => v.TenViTri.ToLower() == vitridoxe.TenViTri.ToLower()))
            {
                ModelState.AddModelError("TenViTri", "Tên vị trí đỗ xe đã tồn tại trong hệ thống!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vitridoxe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", vitridoxe.MaLoaiPhuongTien);
            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Edit/5 chi admin moi dc
        public async Task<IActionResult> Edit(int? mavitri)
        {
            ////admin
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
            if (mavitri == null)
            {
                return NotFound();
            }

            var vitridoxe = await _context.ViTriDoXe.FindAsync(mavitri);
            if (vitridoxe == null)
            {
                return NotFound();
            }
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", vitridoxe.MaLoaiPhuongTien);
            return View(vitridoxe);
        }

        // POST: VITRIDOXES/Edit/5 admin cap nhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? mavitri, [Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
            if (mavitri != vitridoxe.MaViTri)
            {
                return NotFound();
            }
            //valid ten vi tri khong dc trung ngoai tru vi tri dang sua
            if (await _context.ViTriDoXe.AnyAsync(v => v.TenViTri.ToLower() == vitridoxe.TenViTri.ToLower() && v.MaViTri != vitridoxe.MaViTri))
            {
                ModelState.AddModelError("TenViTri", "Tên vị trí đỗ xe đã tồn tại trong hệ thống!");
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
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
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
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "TaiKhoan");
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
