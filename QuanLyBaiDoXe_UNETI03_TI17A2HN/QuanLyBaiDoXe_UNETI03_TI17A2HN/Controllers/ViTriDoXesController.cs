using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using System.Net.Http.Headers;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;

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
        public async Task<IActionResult> Index(
            string searchString,
            int? maLoaiPhuongTien,
            string khuVuc,
            int? tang,
            string trangThai,
            string sortOrder)
        {
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentLoaiPhuongTien"] = maLoaiPhuongTien;
            ViewData["CurrentKhuVuc"] = khuVuc;
            ViewData["CurrentTang"] = tang;
            ViewData["CurrentTrangThai"] = trangThai;
            ViewData["CurrentSort"] = sortOrder;

            await NapDuLieuDropdownLocAsync(maLoaiPhuongTien, khuVuc, tang);
            var query = _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .AsQueryable();

            query = TimKiem(query, searchString);                            
            query = LocDaDieuKien(query, maLoaiPhuongTien, khuVuc, tang, trangThai); 
            query = SapXep(query, sortOrder);                            

            return View(await query.ToListAsync());
        }

        ///Tìm kiếm
        private IQueryable<ViTriDoXe> TimKiem(IQueryable<ViTriDoXe> query, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.TenViTri.Contains(searchString) || v.KhuVuc.Contains(searchString));
            }
            return query;
        }
        //Lọc   
        private IQueryable<ViTriDoXe> LocDaDieuKien(
            IQueryable<ViTriDoXe> query,
            int? maLoaiPhuongTien,
            string khuVuc,
            int? tang,
            string trangThai)
        {
            if (maLoaiPhuongTien.HasValue && maLoaiPhuongTien.Value > 0)
            {
                query = query.Where(v => v.MaLoaiPhuongTien == maLoaiPhuongTien.Value);
            }
            if (!string.IsNullOrEmpty(khuVuc))
            {
                query = query.Where(v => v.KhuVuc == khuVuc);
            }
            if (tang.HasValue)
            {
                query = query.Where(v => v.Tang == tang.Value);
            }
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(v => v.TrangThai == trangThai);
            }

            return query;
        }

     //Sắp xếp
        private IQueryable<ViTriDoXe> SapXep(IQueryable<ViTriDoXe> query, string sortOrder)
        {
            switch (sortOrder)
            {
                case "name_desc":
                    return query.OrderByDescending(v => v.TenViTri);
                case "tang_asc":
                    return query.OrderBy(v => v.Tang);              
                case "tang_desc":
                    return query.OrderByDescending(v => v.Tang);     
                case "name_asc":
                default:
                    return query.OrderBy(v => v.TenViTri);          
            }
        }

        // Hàm phụ trợ: Nạp danh sách các ô chọn Lọc
        private async Task NapDuLieuDropdownLocAsync(int? maLoaiPhuongTien, string khuVuc, int? tang)
        {
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", maLoaiPhuongTien);
            ViewData["KhuVucList"] = new SelectList(await _context.ViTriDoXe.Select(v => v.KhuVuc).Distinct().ToListAsync(), khuVuc);
            ViewData["TangList"] = new SelectList(await _context.ViTriDoXe.Select(v => v.Tang).Distinct().OrderBy(t => t).ToListAsync(), tang);
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
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien");
            return View();
        }

        // POST: VITRIDOXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create([Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            // Validate tên vị trí không được trùng
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
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? mavitri)
        {
            ////admin
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
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? mavitri, [Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (mavitri != vitridoxe.MaViTri)
            {
                return NotFound();
            }

            // Validate tên vị trí không được trùng (ngoại trừ vị trí đang sửa)
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

            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", vitridoxe.MaLoaiPhuongTien);
            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Delete/5
        [PhanQuyen("Admin")]
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
        [PhanQuyen("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? mavitri)
        {
            var vitridoxe = await _context.ViTriDoXe.FindAsync(mavitri);
            if (vitridoxe != null)
            {
                _context.ViTriDoXe.Remove(vitridoxe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ViTriDoXeExists(int? mavitri)
        {
            return _context.ViTriDoXe.Any(e => e.MaViTri == mavitri);
        }
    }
}
