// Họ và tên: Tăng Hoàng Giang
// Mã sinh viên: 23103100031
// Nội dung thực hiện: Xử lý Xác nhận xe vào, xe ra và tính tiền tự động (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Services;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    [PhanQuyen("Admin")]
    public class VaoRaController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;
        private readonly QrVeService _qr;

        public VaoRaController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context, QrVeService qr)
        {
            _context = context;
            _qr = qr;
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

        // ===== Nút bấm thủ công trên trang Điều phối =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanVao(int id)
        {
            var (ok, msg) = await XuLyVaoAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanRa(int id)
        {
            var (ok, msg) = await XuLyRaAsync(id);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ===== QUÉT MÃ QR TẠI CỔNG =====

        // GET: VaoRa/Quet – trang quét QR bằng camera / máy quét mã vạch
        public IActionResult Quet()
        {
            return View();
        }

        // POST: VaoRa/TraCuuQr – đọc mã QR, trả thông tin vé và việc cần làm (cho vào / cho ra)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraCuuQr(string? ma)
        {
            var (phieu, loi) = await TimPhieuTheoQrAsync(ma);
            if (phieu == null)
            {
                return Json(new { ok = false, message = loi });
            }

            string? hanhDong = phieu.TrangThai switch
            {
                "Chờ xác nhận" => "vao",
                "Đang gửi" => "ra",
                _ => null
            };

            if (hanhDong == null)
            {
                string lyDo = phieu.TrangThai == "Đã lấy xe"
                    ? "Vé này đã sử dụng xong (xe đã ra khỏi bãi)."
                    : phieu.TrangThai == "Đã hủy"
                        ? "Vé này đã bị hủy."
                        : "Vé không còn hiệu lực.";
                return Json(new { ok = false, message = lyDo, phieu = MoTaPhieu(phieu, null, false) });
            }

            bool coVeThang = await CoVeThangAsync(phieu.MaPhuongTien, DateTime.Now);

            (double soGio, decimal soTien)? tamTinh = null;
            if (hanhDong == "ra" && phieu.ThoiGianVaoThucTe.HasValue)
            {
                tamTinh = TinhTien(phieu, DateTime.Now);
                if (coVeThang) tamTinh = (tamTinh.Value.soGio, 0m);   // xe có vé tháng: miễn phí
            }

            return Json(new
            {
                ok = true,
                hanhDong,
                message = hanhDong == "vao" ? "Vé hợp lệ – xe chờ vào bãi." : "Vé hợp lệ – xe đang gửi, chuẩn bị ra.",
                phieu = MoTaPhieu(phieu, tamTinh, coVeThang)
            });
        }

        // POST: VaoRa/XacNhanQr – nhân viên bấm xác nhận sau khi quét: tự chọn VÀO hoặc RA theo trạng thái vé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanQr(string? ma)
        {
            var (phieu, loi) = await TimPhieuTheoQrAsync(ma);
            if (phieu == null)
            {
                return Json(new { ok = false, message = loi });
            }

            (bool Ok, string Msg) kq;
            if (phieu.TrangThai == "Chờ xác nhận")
            {
                kq = await XuLyVaoAsync(phieu.MaPhieu);
            }
            else if (phieu.TrangThai == "Đang gửi")
            {
                kq = await XuLyRaAsync(phieu.MaPhieu);
            }
            else
            {
                kq = (false, "Vé không còn hiệu lực (đã sử dụng hoặc đã hủy).");
            }

            return Json(new { ok = kq.Ok, message = kq.Msg });
        }

        // ===== Hàm dùng chung =====

        private async Task<(PhieuGuiXe? Phieu, string Loi)> TimPhieuTheoQrAsync(string? ma)
        {
            int? maPhieu = _qr.GiaiMa(ma);
            if (maPhieu == null)
            {
                return (null, "Mã QR không hợp lệ hoặc không thuộc hệ thống.");
            }

            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt!.ChuPhuongTien)
                .Include(p => p.PhuongTien)
                    .ThenInclude(pt => pt!.LoaiPhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu.Value);

            if (phieu == null)
            {
                return (null, "Không tìm thấy phiếu gửi xe tương ứng với mã QR này.");
            }

            return (phieu, "");
        }

        private static object MoTaPhieu(PhieuGuiXe p, (double soGio, decimal soTien)? tamTinh, bool coVeThang)
        {
            return new
            {
                maPhieu = p.MaPhieu,
                bienSo = p.PhuongTien?.BienSoXe,
                loaiXe = p.PhuongTien?.LoaiPhuongTien?.TenLoaiPhuongTien,
                chuXe = p.PhuongTien?.ChuPhuongTien?.HoTen,
                viTri = p.ViTriDoXe?.TenViTri,
                khuVuc = p.ViTriDoXe?.KhuVuc,
                tang = p.ViTriDoXe?.Tang,
                trangThai = p.TrangThai,
                duKienVao = p.ThoiGianDuKienVao.ToString("dd/MM/yyyy HH:mm"),
                duKienRa = p.ThoiGianDuKienRa.ToString("dd/MM/yyyy HH:mm"),
                vaoThucTe = p.ThoiGianVaoThucTe?.ToString("dd/MM/yyyy HH:mm"),
                donGia = p.DonGiaTheoGio.ToString("N0"),
                veThang = coVeThang,
                tamTinhGio = tamTinh?.soGio,
                tamTinhTien = tamTinh?.soTien.ToString("N0")
            };
        }

        // Xe có vé tháng còn hạn (đã thanh toán, chưa hủy) tại thời điểm thu tiền thì miễn phí
        private async Task<bool> CoVeThangAsync(int maPhuongTien, DateTime luc)
        {
            var ngay = luc.Date;
            return await _context.VeThang.AnyAsync(v =>
                v.MaPhuongTien == maPhuongTien &&
                v.TrangThai == "Hoạt động" &&
                v.NgayBatDau <= ngay && v.NgayKetThuc >= ngay);
        }

        // TÍNH TIỀN THEO BLOCK 30 PHÚT (Module 4)
        // Ví dụ: đỗ 40 phút -> 2 block (1.0 giờ); đỗ 1h15p -> 3 block (1.5 giờ). Tối thiểu 1 block.
        private static (double soGio, decimal soTien) TinhTien(PhieuGuiXe phieu, DateTime ra)
        {
            if (!phieu.ThoiGianVaoThucTe.HasValue || ra <= phieu.ThoiGianVaoThucTe.Value)
            {
                return (0, 0);
            }

            TimeSpan duration = ra - phieu.ThoiGianVaoThucTe.Value;
            double totalBlocks = Math.Ceiling(duration.TotalMinutes / 30.0);
            if (totalBlocks < 1) totalBlocks = 1;

            double totalHours = totalBlocks * 0.5;
            return (totalHours, (decimal)totalHours * phieu.DonGiaTheoGio);
        }

        private async Task<(bool Ok, string Msg)> XuLyVaoAsync(int id)
        {
            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null || phieu.TrangThai != "Chờ xác nhận")
            {
                return (false, "Phiếu không hợp lệ hoặc không ở trạng thái Chờ xác nhận.");
            }

            // 1. Kiểm tra phương tiện có đang bị tạm ngừng hoạt động không
            if (phieu.PhuongTien != null && !phieu.PhuongTien.TrangThai)
            {
                return (false, "Phương tiện này hiện đang ở trạng thái ngừng hoạt động!");
            }

            // 2. Kiểm tra vị trí đỗ có đang bị Tạm ngừng không
            if (phieu.ViTriDoXe != null && phieu.ViTriDoXe.TrangThai == "Tạm ngừng")
            {
                return (false, "Vị trí đỗ này hiện đang Tạm ngừng hoạt động!");
            }

            // 3. Kiểm tra loại phương tiện có phù hợp với vị trí đỗ không
            if (phieu.PhuongTien != null && phieu.ViTriDoXe != null && phieu.PhuongTien.MaLoaiPhuongTien != phieu.ViTriDoXe.MaLoaiPhuongTien)
            {
                return (false, "Loại phương tiện không phù hợp với vị trí đỗ này!");
            }

            // 4. Kiểm tra phương tiện này có đang nằm trong bãi (phiếu khác đang gửi) không
            bool xeDangGui = await _context.PhieuGuiXe
                .AnyAsync(p => p.MaPhuongTien == phieu.MaPhuongTien && p.TrangThai == "Đang gửi");

            if (xeDangGui)
            {
                return (false, "Phương tiện này hiện đang có lượt gửi hoạt động trong bãi!");
            }

            // 5. Kiểm tra vị trí đỗ đã được cấp có bị xe khác chiếm chỗ chưa
            bool viTriBan = await _context.ViTriDoXe
                .AnyAsync(v => v.MaViTri == phieu.MaViTri && v.TrangThai == "Đang sử dụng");

            if (viTriBan)
            {
                return (false, "Vị trí đỗ này hiện đang có xe khác sử dụng!");
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

            return (true, $"Đã xác nhận cho xe {phieu.PhuongTien?.BienSoXe} vào bãi thành công" +
                          (phieu.ViTriDoXe != null ? $" – vị trí {phieu.ViTriDoXe.TenViTri}." : "."));
        }

        private async Task<(bool Ok, string Msg)> XuLyRaAsync(int id)
        {
            var phieu = await _context.PhieuGuiXe
                .Include(p => p.PhuongTien)
                .Include(p => p.ViTriDoXe)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null || phieu.TrangThai != "Đang gửi")
            {
                return (false, "Phiếu không hợp lệ hoặc không ở trạng thái Đang gửi.");
            }

            var bayGio = DateTime.Now;

            // Kiểm tra thời gian ra phải lớn hơn thời gian vào
            if (!phieu.ThoiGianVaoThucTe.HasValue || bayGio <= phieu.ThoiGianVaoThucTe.Value)
            {
                return (false, "Lỗi thời gian: Thời gian ra phải sau thời gian vào.");
            }

            phieu.ThoiGianRaThucTe = bayGio;
            var (totalHours, soTien) = TinhTien(phieu, bayGio);
            bool coVeThang = await CoVeThangAsync(phieu.MaPhuongTien, bayGio);
            if (coVeThang) soTien = 0;   // vé tháng: miễn phí lượt gửi
            phieu.ThanhTien = soTien;

            // Giải phóng vị trí đỗ: Chỉ trả về "Còn trống" nếu vị trí không ở trạng thái "Tạm ngừng"
            if (phieu.ViTriDoXe != null && phieu.ViTriDoXe.TrangThai != "Tạm ngừng")
            {
                phieu.ViTriDoXe.TrangThai = "Còn trống";
            }

            // Đánh dấu phiếu là Đã lấy xe
            phieu.TrangThai = "Đã lấy xe";

            _context.Update(phieu);
            await _context.SaveChangesAsync();

            if (coVeThang)
            {
                return (true, $"Xe {phieu.PhuongTien?.BienSoXe} ra khỏi bãi. Thời gian gửi: {totalHours} giờ. Xe có VÉ THÁNG nên miễn phí (thu 0 VNĐ).");
            }

            return (true, $"Xe {phieu.PhuongTien?.BienSoXe} ra khỏi bãi. Thời gian gửi: {totalHours} giờ (tính theo block 30 phút). Số tiền thu: {phieu.ThanhTien?.ToString("N0")} VNĐ.");
        }
    }
}
