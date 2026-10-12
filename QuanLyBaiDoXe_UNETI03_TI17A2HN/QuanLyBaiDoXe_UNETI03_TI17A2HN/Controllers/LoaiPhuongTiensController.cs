using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;//23103100064 - Lương Thị Quỳnh Hương

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin")]
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

        // GET: LoaiPhuongTiens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhuongTien = await _context.LoaiPhuongTien
                .FirstOrDefaultAsync(m => m.MaLoaiPhuongTien == id);
            if (loaiPhuongTien == null)
            {
                return NotFound();
            }

            return View(loaiPhuongTien);
        }

        // GET: LoaiPhuongTiens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiPhuongTiens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLoaiPhuongTien,TenLoaiPhuongTien,DonGiaTheoGio,MoTa,TrangThai")] LoaiPhuongTien loaiPhuongTien)
        {
            // Kiểm tra tên bắt buộc
            if (string.IsNullOrWhiteSpace(loaiPhuongTien.TenLoaiPhuongTien))
            {
                ModelState.AddModelError("TenLoaiPhuongTien", "Tên loại phương tiện không được để trống.");
            }
            else
            {
                // Kiểm tra tên không được trùng
                bool isTrungTen = await _context.LoaiPhuongTien
                    .AnyAsync(e => e.TenLoaiPhuongTien.ToLower() == loaiPhuongTien.TenLoaiPhuongTien.ToLower());
                if (isTrungTen)
                {
                    ModelState.AddModelError("TenLoaiPhuongTien", "Tên loại phương tiện này đã tồn tại trong hệ thống.");
                }
            }

            // Kiểm tra đơn giá theo giờ phải > 0
            if (loaiPhuongTien.DonGiaTheoGio <= 0)
            {
                ModelState.AddModelError("DonGiaTheoGio", "Đơn giá theo giờ phải lớn hơn 0.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(loaiPhuongTien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiPhuongTien);
        }

        // GET: LoaiPhuongTiens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhuongTien = await _context.LoaiPhuongTien.FindAsync(id);
            if (loaiPhuongTien == null)
            {
                return NotFound();
            }
            return View(loaiPhuongTien);
        }

        // POST: LoaiPhuongTiens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLoaiPhuongTien,TenLoaiPhuongTien,DonGiaTheoGio,MoTa,TrangThai")] LoaiPhuongTien loaiPhuongTien)
        {
            if (id != loaiPhuongTien.MaLoaiPhuongTien)
            {
                return NotFound();
            }

            // Kiểm tra tên bắt buộc
            if (string.IsNullOrWhiteSpace(loaiPhuongTien.TenLoaiPhuongTien))
            {
                ModelState.AddModelError("TenLoaiPhuongTien", "Tên loại phương tiện không được để trống.");
            }
            else
            {
                // Kiểm tra trùng tên (trừ chính nó)
                bool isTrungTen = await _context.LoaiPhuongTien
                    .AnyAsync(e => e.TenLoaiPhuongTien.ToLower() == loaiPhuongTien.TenLoaiPhuongTien.ToLower() && e.MaLoaiPhuongTien != id);
                if (isTrungTen)
                {
                    ModelState.AddModelError("TenLoaiPhuongTien", "Tên loại phương tiện này đã tồn tại.");
                }
            }

            // Kiểm tra đơn giá theo giờ > 0
            if (loaiPhuongTien.DonGiaTheoGio <= 0)
            {
                ModelState.AddModelError("DonGiaTheoGio", "Đơn giá theo giờ phải lớn hơn 0.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiPhuongTien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiPhuongTienExists(loaiPhuongTien.MaLoaiPhuongTien))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(loaiPhuongTien);
        }

        // GET: LoaiPhuongTiens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhuongTien = await _context.LoaiPhuongTien
                .FirstOrDefaultAsync(m => m.MaLoaiPhuongTien == id);
            if (loaiPhuongTien == null)
            {
                return NotFound();
            }

            return View(loaiPhuongTien);
        }

        // POST: LoaiPhuongTiens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiPhuongTien = await _context.LoaiPhuongTien
                .Include(l => l.PhuongTiens)
                .Include(l => l.ViTriDoXes)
                .FirstOrDefaultAsync(m => m.MaLoaiPhuongTien == id);

            if (loaiPhuongTien == null)
            {
                return NotFound();
            }

            // Ràng buộc: Không được xóa nếu còn phương tiện hoặc vị trí đỗ liên quan
            bool coPhuongTien = loaiPhuongTien.PhuongTiens != null && loaiPhuongTien.PhuongTiens.Any();
            bool coViTriDo = loaiPhuongTien.ViTriDoXes != null && loaiPhuongTien.ViTriDoXes.Any();

            if (coPhuongTien || coViTriDo)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa loại phương tiện này vì đang có dữ liệu Phương tiện hoặc Vị trí đỗ liên quan.");
                TempData["ErrorMessage"] = "Không thể xóa do có dữ liệu liên quan!";
                return View(loaiPhuongTien);
            }

            _context.LoaiPhuongTien.Remove(loaiPhuongTien);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: LoaiPhuongTiens/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var item = await _context.LoaiPhuongTien.FindAsync(id);
            if (item != null)
            {
                item.TrangThai = !item.TrangThai;
                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thay đổi trạng thái thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiPhuongTienExists(int? maloaiphuongtien)
        {
            return _context.LoaiPhuongTien.Any(e => e.MaLoaiPhuongTien == maloaiphuongtien);
        }
    }
}
