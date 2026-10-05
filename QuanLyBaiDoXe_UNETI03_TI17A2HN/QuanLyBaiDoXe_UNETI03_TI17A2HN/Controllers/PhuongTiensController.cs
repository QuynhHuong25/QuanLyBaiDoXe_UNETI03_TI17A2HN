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
        public async Task<IActionResult> Index()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                var myVehicles = await _context.PhuongTien
                    .Include(p => p.ChuPhuongTien)
                    .Include(p => p.LoaiPhuongTien)
                    .Where(p => p.ChuPhuongTien.MaTaiKhoan == maTaiKhoan)
                    .ToListAsync();
                return View(myVehicles);
            }

            var quanLyBaiDoXeContext = _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien);
            return View(await quanLyBaiDoXeContext.ToListAsync());
        }

        // GET: PHUONGTIENS/Details/5
        public async Task<IActionResult> Details(int? maphuongtien)
        {
            if (maphuongtien == null)
            {
                return NotFound();
            }

            var phuongtien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(m => m.MaPhuongTien == maphuongtien);

            if (phuongtien == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (phuongtien.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(phuongtien);
        }

        // GET: PHUONGTIENS/Create
        public IActionResult Create()
        {
            ViewData["MaChuPhuongTien"] = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen");
            ViewData["MaLoaiPhuongTien"] = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien");
            return View();
        }

        // POST: PHUONGTIENS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhuongTien,MaChuPhuongTien,MaLoaiPhuongTien,BienSoXe,NhanHieu,MauSac,MoTa,TrangThai")] PhuongTien phuongtien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phuongtien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaChuPhuongTien"] = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongtien.MaChuPhuongTien);
            ViewData["MaLoaiPhuongTien"] = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongtien.MaLoaiPhuongTien);
            return View(phuongtien);
        }

        // GET: PHUONGTIENS/Edit/5
        public async Task<IActionResult> Edit(int? maphuongtien)
        {
            if (maphuongtien == null)
            {
                return NotFound();
            }

            var phuongtien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .FirstOrDefaultAsync(p => p.MaPhuongTien == maphuongtien);

            if (phuongtien == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (phuongtien.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["MaChuPhuongTien"] = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongtien.MaChuPhuongTien);
            ViewData["MaLoaiPhuongTien"] = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongtien.MaLoaiPhuongTien);
            return View(phuongtien);
        }

        // POST: PHUONGTIENS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? maphuongtien, [Bind("MaPhuongTien,MaChuPhuongTien,MaLoaiPhuongTien,BienSoXe,NhanHieu,MauSac,MoTa,TrangThai")] PhuongTien phuongtien)
        {
            if (maphuongtien != phuongtien.MaPhuongTien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phuongtien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhuongTienExists(phuongtien.MaPhuongTien))
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
            ViewData["MaChuPhuongTien"] = new SelectList(_context.ChuPhuongTien, "MaChuPhuongTien", "HoTen", phuongtien.MaChuPhuongTien);
            ViewData["MaLoaiPhuongTien"] = new SelectList(_context.LoaiPhuongTien, "MaLoaiPhuongTien", "TenLoaiPhuongTien", phuongtien.MaLoaiPhuongTien);
            return View(phuongtien);
        }

        // GET: PHUONGTIENS/Delete/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Delete(int? maphuongtien)
        {
            if (maphuongtien == null)
            {
                return NotFound();
            }

            var phuongtien = await _context.PhuongTien
                .Include(p => p.ChuPhuongTien)
                .Include(p => p.LoaiPhuongTien)
                .FirstOrDefaultAsync(m => m.MaPhuongTien == maphuongtien);

            if (phuongtien == null)
            {
                return NotFound();
            }

            return View(phuongtien);
        }

        // POST: PHUONGTIENS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? maphuongtien)
        {
            var phuongtien = await _context.PhuongTien.FindAsync(maphuongtien);
            if (phuongtien != null)
            {
                _context.PhuongTien.Remove(phuongtien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhuongTienExists(int? maphuongtien)
        {
            return _context.PhuongTien.Any(e => e.MaPhuongTien == maphuongtien);
        }
    }
}
