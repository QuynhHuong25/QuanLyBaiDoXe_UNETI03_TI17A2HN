using Microsoft.AspNetCore.Identity;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>
    /// Mã hóa mật khẩu bằng bộ băm của ASP.NET Core Identity (PasswordHasher: PBKDF2 + salt ngẫu nhiên).
    /// Tương thích dữ liệu cũ: mật khẩu còn lưu dạng chữ thường (vd. "123456") vẫn đăng nhập được
    /// và sẽ được tự động băm lại ngay sau lần đăng nhập thành công đầu tiên.
    /// </summary>
    public class MatKhauService
    {
        private readonly PasswordHasher<TaiKhoan> _hasher = new();

        /// <summary>Băm mật khẩu để lưu vào cột MatKhau (khoảng 84 ký tự, vừa với StringLength(100)).</summary>
        public string Bam(TaiKhoan taiKhoan, string matKhau) => _hasher.HashPassword(taiKhoan, matKhau);

        /// <summary>Đã là chuỗi băm của Identity (định dạng v3 luôn bắt đầu bằng "AQAAAA") hay còn là chữ thường?</summary>
        public static bool DaBam(string? giaTri) =>
            !string.IsNullOrEmpty(giaTri) && giaTri.Length >= 60 && giaTri.StartsWith("AQAAAA", StringComparison.Ordinal);

        /// <summary>
        /// Kiểm tra mật khẩu. canBamLai = true nghĩa là nên lưu lại bản băm mới
        /// (mật khẩu cũ dạng chữ thường hoặc băm bằng thuật toán yếu hơn).
        /// </summary>
        public (bool hopLe, bool canBamLai) KiemTra(TaiKhoan taiKhoan, string matKhauNhap)
        {
            var daLuu = taiKhoan.MatKhau;
            if (string.IsNullOrEmpty(daLuu) || string.IsNullOrEmpty(matKhauNhap)) return (false, false);

            if (!DaBam(daLuu))
            {
                // Dữ liệu cũ chưa mã hóa: so sánh trực tiếp rồi yêu cầu băm lại
                return (string.Equals(daLuu, matKhauNhap, StringComparison.Ordinal), true);
            }

            var kq = _hasher.VerifyHashedPassword(taiKhoan, daLuu, matKhauNhap);
            return kq switch
            {
                PasswordVerificationResult.Success => (true, false),
                PasswordVerificationResult.SuccessRehashNeeded => (true, true),
                _ => (false, false)
            };
        }
    }
}