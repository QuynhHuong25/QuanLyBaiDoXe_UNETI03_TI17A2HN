namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>
    /// Lưu / xóa ảnh phương tiện trong wwwroot/uploads/phuongtien.
    /// Kiểm tra: dung lượng, đuôi file và chữ ký nhị phân thật của ảnh (không tin vào tên file người dùng gửi).
    /// </summary>
    public class AnhPhuongTienService
    {
        private const long ToiDa = 3 * 1024 * 1024; // 3 MB
        private const string ThuMuc = "uploads/phuongtien";
        private readonly IWebHostEnvironment _env;

        public AnhPhuongTienService(IWebHostEnvironment env) => _env = env;

        public async Task<(string? duongDan, string? loi)> LuuAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return (null, null);

            if (file.Length > ToiDa) return (null, "Ảnh không được vượt quá 3 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                return (null, "Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.");

            // Đọc phần đầu file để xác thực định dạng thật
            var dau = new byte[12];
            await using (var rs = file.OpenReadStream())
            {
                var doc = await rs.ReadAsync(dau.AsMemory(0, dau.Length));
                if (doc < 12) return (null, "File ảnh không hợp lệ.");
            }

            bool jpg = dau[0] == 0xFF && dau[1] == 0xD8 && dau[2] == 0xFF;
            bool png = dau[0] == 0x89 && dau[1] == 0x50 && dau[2] == 0x4E && dau[3] == 0x47;
            bool webp = dau[0] == 0x52 && dau[1] == 0x49 && dau[2] == 0x46 && dau[3] == 0x46 &&
                        dau[8] == 0x57 && dau[9] == 0x45 && dau[10] == 0x42 && dau[11] == 0x50;
            if (!(jpg || png || webp)) return (null, "Nội dung file không phải là ảnh hợp lệ.");

            var thuMucVatLy = Path.Combine(_env.WebRootPath, ThuMuc.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(thuMucVatLy);

            // Tên file do hệ thống sinh ra: tránh trùng tên và tránh ký tự nguy hiểm trong đường dẫn
            var ten = Guid.NewGuid().ToString("N") + (jpg ? ".jpg" : png ? ".png" : ".webp");
            await using (var fs = new FileStream(Path.Combine(thuMucVatLy, ten), FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return ($"/{ThuMuc}/{ten}", null);
        }

        public void Xoa(string? duongDan)
        {
            if (string.IsNullOrWhiteSpace(duongDan) || !duongDan.StartsWith($"/{ThuMuc}/", StringComparison.Ordinal)) return;
            var ten = Path.GetFileName(duongDan);                  // chỉ lấy tên file, chặn "../"
            var full = Path.Combine(_env.WebRootPath, ThuMuc.Replace('/', Path.DirectorySeparatorChar), ten);
            try { if (File.Exists(full)) File.Delete(full); } catch { /* bỏ qua lỗi xóa file */ }
        }
    }
}
