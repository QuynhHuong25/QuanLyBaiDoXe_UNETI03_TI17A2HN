// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Xử lý Xác nhận xe vào, xe ra và tính tiền tự động (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin")]
    public class VaoRaController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public VaoRaController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchBienSo)
        {
            ViewData["searchBienSo"] = searchBienSo;

            // Chỉ lấy các phiếu Chờ xác nhận hoặc Đang gửi để xử lý VÀO/RA
            var query = _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.ChuPhuongTien)
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .Where(p => p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang gửi")
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchBienSo))
            {
                query = query.Where(p => p.PhuongTien.BienSoXe.Contains(searchBienSo));
            }

            // Sắp xếp: Chờ xác nhận lên trên, rồi Đang gửi
            query = query.OrderByDescending(p => p.TrangThai == "Chờ xác nhận")
                         .ThenBy(p => p.ThoiGianDuKienVao);

            return View(await query.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanVao(int id)
        {
            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null || phieu.TrangThai != "Chờ xác nhận")
            {
                TempData["ErrorMessage"] = "Phiếu không hợp lệ hoặc không ở trạng thái Chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Kiểm tra phương tiện có đang bị tạm ngừng hoạt động không
            if (phieu.PhuongTien != null && !phieu.PhuongTien.TrangThai)
            {
                TempData["ErrorMessage"] = "Phương tiện này hiện đang ở trạng thái ngừng hoạt động!";
                return RedirectToAction(nameof(Index));
            }

            // 2. Kiểm tra vị trí đỗ có đang bị Tạm ngừng không
            if (phieu.ViTriDoXe != null && phieu.ViTriDoXe.TrangThai == "Tạm ngừng")
            {
                TempData["ErrorMessage"] = "Vị trí đỗ này hiện đang Tạm ngừng hoạt động!";
                return RedirectToAction(nameof(Index));
            }

            // 3. Kiểm tra loại phương tiện có phù hợp với vị trí đỗ không
            if (phieu.PhuongTien != null && phieu.ViTriDoXe != null && phieu.PhuongTien.MaLoaiPhuongTien != phieu.ViTriDoXe.MaLoaiPhuongTien)
            {
                TempData["ErrorMessage"] = "Loại phương tiện không phù hợp với vị trí đỗ này!";
                return RedirectToAction(nameof(Index));
            }

            // 4. Kiểm tra phương tiện này có đang nằm trong bãi (phiếu khác đang gửi) không
            bool xeDangGui = await _context.PhieuGuiXe
                .AnyAsync(p => p.MaPhuongTien == phieu.MaPhuongTien && p.TrangThai == "Đang gửi");

            if (xeDangGui)
            {
                TempData["ErrorMessage"] = "Phương tiện này hiện đang có lượt gửi hoạt động trong bãi!";
                return RedirectToAction(nameof(Index));
            }

            // 5. Kiểm tra vị trí đỗ đã được cấp có bị xe khác chiếm chỗ chưa
            bool viTriBan = await _context.ViTriDoXe
                .AnyAsync(v => v.MaViTri == phieu.MaViTri && v.TrangThai == "Đang sử dụng");

            if (viTriBan)
            {
                TempData["ErrorMessage"] = "Vị trí đỗ này hiện đang có xe khác sử dụng!";
                return RedirectToAction(nameof(Index));
            }

            // Xác nhận vào: Ghi nhận giờ vào thực tế và đổi trạng thái
            phieu.ThoiGianVaoThucTe = DateTime.Now;
            phieu.TrangThai = "Đang gửi";

            // Đổi trạng thái vị trí đỗ thành Đang sử dụng
            if (phieu.ViTriDoXe != null)
            {
                phieu.ViTriDoXe.TrangThai = "Đang sử dụng";
            }

            _context.Update(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xác nhận cho xe {phieu.PhuongTien?.BienSoXe} vào bãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanRa(int id)
        {
            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null || phieu.TrangThai != "Đang gửi")
            {
                TempData["ErrorMessage"] = "Phiếu không hợp lệ hoặc không ở trạng thái Đang gửi.";
                return RedirectToAction(nameof(Index));
            }

            phieu.ThoiGianRaThucTe = DateTime.Now;

            double totalHours = 0;

            // Kiểm tra thời gian ra phải lớn hơn thời gian vào
            if (phieu.ThoiGianVaoThucTe.HasValue && phieu.ThoiGianRaThucTe > phieu.ThoiGianVaoThucTe)
            {
                // Tính khoảng thời gian đỗ xe
                TimeSpan duration = phieu.ThoiGianRaThucTe.Value - phieu.ThoiGianVaoThucTe.Value;

                // TÍNH TIỀN THEO BLOCK 30 PHÚT (Module 4)
                // Ví dụ: đỗ 40 phút -> làm tròn lên thành 2 block (60 phút = 1.0 giờ)
                // đỗ 1h15p (75 phút) -> làm tròn lên thành 3 block (90 phút = 1.5 giờ)
                double totalBlocks = Math.Ceiling(duration.TotalMinutes / 30.0);

                // Tối thiểu thu phí 1 block 30 phút (0.5 giờ)
                if (totalBlocks < 1) totalBlocks = 1;

                totalHours = totalBlocks * 0.5;

                // Tính thành tiền = Số giờ quy đổi x Đơn giá theo giờ
                phieu.ThanhTien = (decimal)totalHours * phieu.DonGiaTheoGio;
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi thời gian: Thời gian ra phải sau thời gian vào.";
                return RedirectToAction(nameof(Index));
            }

            // Giải phóng vị trí đỗ: Chỉ trả về "Còn trống" nếu vị trí không ở trạng thái "Tạm ngừng"
            if (phieu.ViTriDoXe != null && phieu.ViTriDoXe.TrangThai != "Tạm ngừng")
            {
                phieu.ViTriDoXe.TrangThai = "Còn trống";
            }

            // Đánh dấu phiếu là Đã lấy xe
            phieu.TrangThai = "Đã lấy xe";

            _context.Update(phieu);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Xe {phieu.PhuongTien?.BienSoXe} ra khỏi bãi. Thời gian gửi: {totalHours} giờ (tính theo block 30 phút). Số tiền thu: {phieu.ThanhTien?.ToString("N0")} VNĐ.";
            return RedirectToAction(nameof(Index));
        }
    }
}
