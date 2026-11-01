using System.Security.Cryptography;
using System.Text;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>
    /// Tạo / kiểm tra mã QR của vé gửi xe.
    /// Nội dung QR có dạng  PK-{MaPhieu}-{chữ ký}  (ví dụ PK-12-3fa9c01b7d52).
    /// Chữ ký là HMAC-SHA256(MaPhieu) bằng khóa bí mật trong appsettings ("Qr:Secret"),
    /// nên không ai đoán / làm giả được mã của phiếu khác. Không cần thêm cột vào CSDL.
    /// </summary>
    public class QrVeService
    {
        private const string TienTo = "PK";
        private const int DoDaiChuKy = 12; // số ký tự hex giữ lại

        private readonly byte[] _khoa;

        public QrVeService(IConfiguration config)
        {
            var secret = config["Qr:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                secret = "ParkSmart-UNETI03-doi-khoa-nay-trong-appsettings";
            }
            _khoa = Encoding.UTF8.GetBytes(secret);
        }

        public string Tao(int maPhieu)
        {
            return $"{TienTo}-{maPhieu}-{KyTen(maPhieu)}";
        }

        /// <summary>Trả về mã phiếu nếu mã QR hợp lệ, ngược lại trả null.</summary>
        public int? GiaiMa(string? noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung)) return null;

            var phan = noiDung.Trim().Split('-');
            if (phan.Length != 3 || !phan[0].Equals(TienTo, StringComparison.OrdinalIgnoreCase)) return null;
            if (!int.TryParse(phan[1], out int maPhieu) || maPhieu <= 0) return null;

            var dung = KyTen(maPhieu);
            var nhan = phan[2].ToLowerInvariant();

            bool khop = CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(dung),
                Encoding.ASCII.GetBytes(nhan));

            return khop ? maPhieu : null;
        }

        private string KyTen(int maPhieu)
        {
            using var hmac = new HMACSHA256(_khoa);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("phieu:" + maPhieu));
            return Convert.ToHexString(hash).ToLowerInvariant().Substring(0, DoDaiChuKy);
        }
    }
}
