using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

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
           string sortOrder,
           int page = 1)
        {
            // TỰ ĐỘNG ĐỒNG BỘ TRẠNG THÁI VỊ TRÍ ĐỖ XE THEO BẢNG PHIẾU GỬI
            await DongBoTrangThaiViTriAsync();

            int pageSize = 10;

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

            // Tìm kiếm
            query = TimKiem(query, searchString);

            // Lọc đa điều kiện
            query = LocDaDieuKien(query, maLoaiPhuongTien, khuVuc, tang, trangThai);

            // Sắp xếp
            query = SapXep(query, sortOrder);

            // Phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentLoaiPhuongTien = maLoaiPhuongTien;
            ViewBag.CurrentKhuVuc = khuVuc;
            ViewBag.CurrentTang = tang;
            ViewBag.CurrentTrangThai = trangThai;
            ViewBag.CurrentSort = sortOrder;

            // Phân trang bằng Skip và Take
            var items = await PhanTrang(query, page, pageSize).ToListAsync();

            // Sơ đồ đỗ xe
            var soDoList = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .OrderBy(v => v.Tang)
                .ThenBy(v => v.KhuVuc)
                .ThenBy(v => v.TenViTri)
                .ToListAsync();

            ViewBag.SoDo = soDoList;

            // Xác định tầng mặc định hiển thị trên tab sơ đồ
            ViewBag.ActiveTang = tang.HasValue ? tang.Value : (soDoList.Select(s => s.Tang).FirstOrDefault());

            // LẤY BIỂN SỐ XE CHO CÁC PHIẾU ĐANG GỬI (ĐÃ ÉP KIỂU CHIẾT XUẤT CHÍNH XÁC Dictionary<int, string>)
            var phieuDangGuiList = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Where(p => p.TrangThai == "Đang gửi" && p.MaViTri != null)
                .Select(p => new { MaViTri = p.MaViTri!.Value, BienSoXe = p.PhuongTien != null ? p.PhuongTien.BienSoXe : "" })
                .ToListAsync();

            ViewBag.PhieuDangGui = phieuDangGuiList
                .GroupBy(x => x.MaViTri)
                .ToDictionary(g => g.Key, g => g.First().BienSoXe ?? "");

            return View(items);
        }

        /// <summary>
        /// Tự động đồng bộ lại toàn bộ trạng thái trong bảng ViTriDoXe 
        /// dựa theo danh sách các phiếu xe thực sự đang đỗ (TrangThai == "Đang gửi")
        /// </summary>
        private async Task DongBoTrangThaiViTriAsync()
        {
            var maViTriDangCoXe = await _context.PhieuGuiXe
                .Where(p => p.TrangThai == "Đang gửi" && p.MaViTri != null)
                .Select(p => p.MaViTri!.Value)
                .Distinct()
                .ToListAsync();

            var danhSachViTri = await _context.ViTriDoXe
                .Where(v => v.TrangThai != "Tạm ngừng")
                .ToListAsync();

            bool coThayDoi = false;

            foreach (var vt in danhSachViTri)
            {
                if (maViTriDangCoXe.Contains(vt.MaViTri))
                {
                    if (vt.TrangThai != "Đang sử dụng")
                    {
                        vt.TrangThai = "Đang sử dụng";
                        coThayDoi = true;
                    }
                }
                else
                {
                    if (vt.TrangThai != "Còn trống")
                    {
                        vt.TrangThai = "Còn trống";
                        coThayDoi = true;
                    }
                }
            }

            if (coThayDoi)
            {
                await _context.SaveChangesAsync();
            }
        }

        private IQueryable<ViTriDoXe> PhanTrang(IQueryable<ViTriDoXe> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private IQueryable<ViTriDoXe> TimKiem(IQueryable<ViTriDoXe> query, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.Trim().ToLower();
                query = query.Where(v => (v.TenViTri != null && v.TenViTri.ToLower().Contains(searchLower)) ||
                                         (v.KhuVuc != null && v.KhuVuc.ToLower().Contains(searchLower)));
            }
            return query;
        }

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

        // ACTION LAY SO DO DANG PARTIAL VIEW (Load AJAX / Modal Popup)
        [PhanQuyen("Admin", "Khách hàng")]
        public async Task<IActionResult> SoDoPartial(int tang = 0)
        {
            await DongBoTrangThaiViTriAsync();

            var ds = await _context.ViTriDoXe
                .Include(v => v.LoaiPhuongTien)
                .OrderBy(v => v.Tang).ThenBy(v => v.KhuVuc).ThenBy(v => v.TenViTri)
                .ToListAsync();

            var xeDangGuiList = await _context.PhieuGuiXe
                .Where(p => p.TrangThai == "Đang gửi" && p.MaViTri != null)
                .Select(p => new { MaViTri = p.MaViTri!.Value, BienSoXe = p.PhuongTien != null ? p.PhuongTien.BienSoXe : "" })
                .ToListAsync();

            ViewBag.XeDangGui = xeDangGuiList
                .GroupBy(x => x.MaViTri)
                .ToDictionary(g => g.Key, g => g.First().BienSoXe ?? "");

            ViewBag.TangMo = tang;
            return PartialView("_SoDoBaiDo", ds);
        }

        // GET: VITRIDOXES/Create (Admin)
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachKhuAsync();
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien");
            return View();
        }

        // ĐỔI TRẠNG THÁI TẠM NGỪNG (Chỉ cho phép khi vị trí CÒN TRỐNG)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> ToggleTamNgung(int id)
        {
            var viTri = await _context.ViTriDoXe.FindAsync(id);
            if (viTri == null)
            {
                return NotFound();
            }

            if (viTri.TrangThai == "Tạm ngừng")
            {
                viTri.TrangThai = "Còn trống";
                TempData["SuccessMessage"] = $"Đã mở lại hoạt động cho vị trí {viTri.TenViTri}.";
            }
            else
            {
                // RÀNG BUỘC CHỈ TẠM NGỪNG KHI CÒN TRỐNG
                if (viTri.TrangThai == "Đang sử dụng")
                {
                    TempData["ErrorMessage"] = $"Không thể tạm ngừng vị trí {viTri.TenViTri} vì hiện đang có xe đỗ!";
                    return RedirectToAction(nameof(Index));
                }

                viTri.TrangThai = "Tạm ngừng";
                TempData["SuccessMessage"] = $"Đã chuyển vị trí {viTri.TenViTri} sang trạng thái Tạm ngừng.";
            }

            _context.Update(viTri);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: VITRIDOXES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create([Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (await _context.ViTriDoXe.AnyAsync(v => v.TenViTri.ToLower() == vitridoxe.TenViTri.ToLower()))
            {
                ModelState.AddModelError("TenViTri", "Tên vị trí đỗ xe đã tồn tại trong hệ thống!");
            }

            await KiemTraQuyDinhTangAsync(vitridoxe);

            if (ModelState.IsValid)
            {
                if (vitridoxe.TrangThai != "Tạm ngừng")
                {
                    vitridoxe.TrangThai = "Còn trống";
                }

                _context.Add(vitridoxe);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tạo thành công vị trí {vitridoxe.TenViTri}!";
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachKhuAsync();
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", vitridoxe.MaLoaiPhuongTien);
            return View(vitridoxe);
        }

        // GET: VITRIDOXES/Edit/5
        [PhanQuyen("Admin")]
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
            await NapDanhSachKhuAsync();
            ViewData["MaLoaiPhuongTien"] = new SelectList(await _context.LoaiPhuongTien.ToListAsync(), "MaLoaiPhuongTien", "TenLoaiPhuongTien", vitridoxe.MaLoaiPhuongTien);
            return View(vitridoxe);
        }

        // POST: VITRIDOXES/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? mavitri, [Bind("MaViTri,TenViTri,MaLoaiPhuongTien,KhuVuc,Tang,TrangThai,MoTa")] ViTriDoXe vitridoxe)
        {
            if (mavitri != vitridoxe.MaViTri)
            {
                return NotFound();
            }

            if (await _context.ViTriDoXe.AnyAsync(v => v.TenViTri.ToLower() == vitridoxe.TenViTri.ToLower() && v.MaViTri != vitridoxe.MaViTri))
            {
                ModelState.AddModelError("TenViTri", "Tên vị trí đỗ xe đã tồn tại trong hệ thống!");
            }

            // Kiểm tra xem vị trí có xe đỗ không
            bool xeDangDo = await _context.PhieuGuiXe
                .AnyAsync(p => p.MaViTri == vitridoxe.MaViTri && p.TrangThai == "Đang gửi");

            // RÀNG BUỘC KHI SỬA THÔNG TIN: Nếu vị trí đang có xe đỗ thì ép trạng thái giữ nguyên "Đang sử dụng"
            if (xeDangDo)
            {
                vitridoxe.TrangThai = "Đang sử dụng";
            }
            else if (vitridoxe.TrangThai != "Tạm ngừng")
            {
                vitridoxe.TrangThai = "Còn trống";
            }

            await KiemTraQuyDinhTangAsync(vitridoxe);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vitridoxe);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật vị trí {vitridoxe.TenViTri} thành công!";
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

            await NapDanhSachKhuAsync();
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
                TempData["SuccessMessage"] = "Xóa vị trí đỗ xe thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Danh sách khu vực hiện có để đổ vào combobox "Khu vực"
        private async Task NapDanhSachKhuAsync()
        {
            ViewBag.DanhSachKhu = (await _context.ViTriDoXe
                    .Where(v => v.KhuVuc != null && v.KhuVuc != "")
                    .Select(v => v.KhuVuc!)
                    .Distinct()
                    .ToListAsync())
                .OrderBy(k => k)
                .ToList();
        }

        // Tầng 2 trở lên chỉ dành cho ô tô và xe khách; ô tô / xe khách chỉ đặt từ tầng 2 trở lên
        private async Task KiemTraQuyDinhTangAsync(ViTriDoXe vitridoxe)
        {
            var loai = await _context.LoaiPhuongTien.FirstOrDefaultAsync(l => l.MaLoaiPhuongTien == vitridoxe.MaLoaiPhuongTien);
            bool laOto = loai != null && QuyDinhBaiXe.LaOtoHoacXeKhach(loai.TenLoaiPhuongTien);

            if (vitridoxe.Tang >= QuyDinhBaiXe.TangDanhRieng && !laOto)
            {
                ModelState.AddModelError("MaLoaiPhuongTien", $"Tầng {vitridoxe.Tang} chỉ dành cho ô tô và xe khách. Xe máy / xe điện vui lòng đặt ở Tầng 1.");
            }
            else if (vitridoxe.Tang < QuyDinhBaiXe.TangDanhRieng && laOto)
            {
                ModelState.AddModelError("Tang", $"Ô tô và xe khách chỉ được đặt từ Tầng {QuyDinhBaiXe.TangDanhRieng} trở lên.");
            }
        }

        private bool ViTriDoXeExists(int? mavitri)
        {
            return _context.ViTriDoXe.Any(e => e.MaViTri == mavitri);
        }
    }
}
