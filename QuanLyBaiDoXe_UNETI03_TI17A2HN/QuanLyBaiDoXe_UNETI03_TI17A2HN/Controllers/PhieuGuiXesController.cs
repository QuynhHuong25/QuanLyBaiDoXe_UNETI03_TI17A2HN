// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Quản lý phiếu gửi xe, Lọc, Tìm kiếm, Hủy phiếu (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin", "Khách hàng")]
    public class PhieuGuiXesController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public PhieuGuiXesController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
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
                .ThenInclude(pt => pt.ChuPhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(m => m.MaPhieu == maphieu);

            if (phieuguixe == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                if (phieuguixe.PhuongTien?.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return RedirectToAction(nameof(Index));
                }
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
        [PhanQuyen("Admin")]
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
        [PhanQuyen("Admin")]
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
        [PhanQuyen("Admin")]
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
        [PhanQuyen("Admin")]
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


        // QUẢN LÝ DANH SÁCH, LỌC VÀ TÌM KIẾM (Module 4)
        // GET: PHIEUGUIXES
        public async Task<IActionResult> Index(string searchBienSo, string searchTenChuXe, string filterTrangThai, string filterLoaiXe, DateTime? filterNgayDangKy)
        {
            ViewData["searchBienSo"] = searchBienSo;
            ViewData["searchTenChuXe"] = searchTenChuXe;
            ViewData["filterTrangThai"] = filterTrangThai;
            ViewData["filterLoaiXe"] = filterLoaiXe;
            ViewData["filterNgayDangKy"] = filterNgayDangKy?.ToString("yyyy-MM-dd");
            
            ViewBag.LoaiXeList = await _context.LoaiPhuongTien.ToListAsync();

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            var query = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.ChuPhuongTien)
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .AsQueryable();

            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                query = query.Where(p => p.PhuongTien.ChuPhuongTien.MaTaiKhoan == maTaiKhoan);
            }

            // Tìm kiếm theo Biển số xe (Contains = tìm kiếm tương đối)
            if (!string.IsNullOrEmpty(searchBienSo))
            {
                query = query.Where(p => p.PhuongTien.BienSoXe.Contains(searchBienSo));
            }
            // Tìm kiếm theo Tên chủ xe
            if (!string.IsNullOrEmpty(searchTenChuXe))
            {
                query = query.Where(p => p.PhuongTien.ChuPhuongTien.HoTen.Contains(searchTenChuXe));
            }
            
            // Lọc chính xác theo Trạng thái phiếu
            if (!string.IsNullOrEmpty(filterTrangThai))
            {
                query = query.Where(p => p.TrangThai == filterTrangThai);
            }
            // Lọc chính xác theo Loại xe
            if (!string.IsNullOrEmpty(filterLoaiXe))
            {
                query = query.Where(p => p.PhuongTien.LoaiPhuongTien.TenLoaiPhuongTien == filterLoaiXe);
            }
            // Lọc chính xác theo Ngày đăng ký
            if (filterNgayDangKy.HasValue)
            {
                query = query.Where(p => p.NgayDangKy.Date == filterNgayDangKy.Value.Date);
            }

            // Sắp xếp các phiếu mới nhất (vừa đăng ký) lên đầu bảng để dễ quản lý
            query = query.OrderByDescending(p => p.NgayDangKy);

            return View(await query.ToListAsync());
        }

        // HỦY PHIẾU (Module 4)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu(int id)
        {
            var phieu = await _context.PhieuGuiXe.FindAsync(id);
            if (phieu == null) return NotFound();

            // Phân quyền: Đảm bảo Khách hàng KHÔNG thể dùng Inspect HTML để sửa ID và hủy phiếu của người khác
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
            if (vaiTro == "Khách hàng" && int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
            {
                var phuongTien = await _context.PhuongTien
                    .Include(pt => pt.ChuPhuongTien)
                    .FirstOrDefaultAsync(pt => pt.MaPhuongTien == phieu.MaPhuongTien);
                    
                if (phuongTien?.ChuPhuongTien?.MaTaiKhoan != maTaiKhoan)
                {
                    return Unauthorized(); 
                }
            }

            // Chỉ phiếu chưa được xác nhận vào bãi mới được hủy
            if (phieu.TrangThai != "Chờ xác nhận")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy phiếu đang ở trạng thái 'Chờ xác nhận'.";
                return RedirectToAction(nameof(Index));
            }

            // Thực hiện đổi trạng thái thành Đã hủy (Soft delete - Không xóa hẳn khỏi database)
            phieu.TrangThai = "Đã hủy";
            _context.Update(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy phiếu thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
