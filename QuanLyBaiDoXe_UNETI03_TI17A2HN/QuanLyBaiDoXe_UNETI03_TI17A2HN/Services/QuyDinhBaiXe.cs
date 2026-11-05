using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>
    /// Quy định bố trí bãi xe: Tầng 2 trở lên chỉ dành cho ô tô và xe khách.
    /// </summary>
    public static class QuyDinhBaiXe
    {
        public const int TangDanhRieng = 2; // từ tầng này trở lên chỉ để ô tô / xe khách
        public const string KhuXeKhach = "Khu E - Xe khách";

        public static bool LaOtoHoacXeKhach(string? tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai)) return false;
            var t = tenLoai.Trim().ToLowerInvariant();
            return t.Contains("ô tô") || t.Contains("oto") || t.Contains("xe khách") || t.Contains("khách");
        }

        /// <summary>Xe khách / ô tô trên 7 chỗ (không phải ô tô 4-7 chỗ, ô tô điện).</summary>
        public static bool LaXeKhach(string? tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai)) return false;
            var t = tenLoai.Trim().ToLowerInvariant();
            return t.Contains("trên 7") || t.Contains("khách");
        }

        /// <summary>Hàng của vị trí = phần trước dấu '-' (A1-01 -> A1).</summary>
        public static string LayHang(string? tenViTri)
        {
            if (string.IsNullOrWhiteSpace(tenViTri)) return "";
            var i = tenViTri.IndexOf('-');
            return (i > 0 ? tenViTri[..i] : tenViTri).Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Chạy mỗi lần khởi động (an toàn khi chạy lặp):
        /// - Ô tô / xe khách ở Tầng 1 -> Tầng 2; xe máy, xe điện... ở tầng 2 trở lên -> Tầng 1.
        /// - Xe khách (ô tô trên 7 chỗ) được gom về khu riêng, không lẫn với Khu C.
        /// - Dọn các ô mẫu E/F do phiên bản trước tự tạo (nếu chưa có phiếu nào dùng).
        /// </summary>
        public static async Task DamBaoTangHaiAsync(QuanLyBaiDoXe_UNETI03_TI17A2HNContext db)
        {
            // 1) Dọn ô mẫu cũ do hệ thống tự tạo (mô tả "Tầng 2 - hàng E1/E2/F1/F2"), chỉ xóa khi chưa có phiếu
            var mau = await db.ViTriDoXe
                .Where(v => v.MoTa != null && v.MoTa.StartsWith("Tầng 2 - hàng "))
                .ToListAsync();
            if (mau.Count > 0)
            {
                var ids = mau.Select(v => v.MaViTri).ToList();
                var daDung = await db.PhieuGuiXe.Where(p => p.MaViTri != null && ids.Contains(p.MaViTri.Value))
                    .Select(p => p.MaViTri!.Value).Distinct().ToListAsync();
                var xoa = mau.Where(v => !daDung.Contains(v.MaViTri)).ToList();
                if (xoa.Count > 0)
                {
                    db.ViTriDoXe.RemoveRange(xoa);
                    await db.SaveChangesAsync();
                }
            }

            // 2) Sắp xếp lại tầng
            var tatCa = await db.ViTriDoXe.Include(v => v.LoaiPhuongTien).ToListAsync();
            bool thayDoi = false;
            foreach (var v in tatCa)
            {
                bool laOto = LaOtoHoacXeKhach(v.LoaiPhuongTien?.TenLoaiPhuongTien);
                if (laOto && v.Tang < TangDanhRieng) { v.Tang = TangDanhRieng; thayDoi = true; }
                else if (!laOto && v.Tang >= TangDanhRieng) { v.Tang = 1; thayDoi = true; }
            }

            // 3) Gom xe khách về khu riêng, đặt tên theo hàng E1-01, E1-02...
            var tenDaCo = tatCa.Select(v => (v.TenViTri ?? "").ToLowerInvariant()).ToHashSet();
            var xeKhach = tatCa
                .Where(v => LaXeKhach(v.LoaiPhuongTien?.TenLoaiPhuongTien) && v.KhuVuc != KhuXeKhach)
                .OrderBy(v => v.TenViTri).ToList();
            int so = 1;
            foreach (var v in xeKhach)
            {
                string ten;
                do { ten = $"E1-{so++:00}"; } while (tenDaCo.Contains(ten.ToLowerInvariant()));
                tenDaCo.Add(ten.ToLowerInvariant());
                v.KhuVuc = KhuXeKhach;
                v.TenViTri = ten;
                thayDoi = true;
            }

            if (thayDoi) await db.SaveChangesAsync();
        }
    }
}
