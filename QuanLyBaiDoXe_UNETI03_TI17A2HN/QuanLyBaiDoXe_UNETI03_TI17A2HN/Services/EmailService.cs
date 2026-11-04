using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>Cấu hình SMTP, đọc từ mục "Email" trong appsettings.json</summary>
    public class EmailSettings
    {
        public bool Enabled { get; set; } = false;      // false = không gửi (chỉ ghi log)
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";       // với Gmail: dùng "Mật khẩu ứng dụng" (App Password)
        public string FromName { get; set; } = "ParkSmart";
    }

    /// <summary>Gửi email xác nhận (dùng System.Net.Mail, không cần gói NuGet).</summary>
    public class EmailService
    {
        private readonly EmailSettings _cfg;
        private readonly ILogger<EmailService> _log;

        public EmailService(IOptions<EmailSettings> cfg, ILogger<EmailService> log)
        {
            _cfg = cfg.Value;
            _log = log;
        }

        public async Task<bool> GuiAsync(string? den, string tieuDe, string noiDungHtml)
        {
            if (string.IsNullOrWhiteSpace(den)) return false;

            if (!_cfg.Enabled || string.IsNullOrWhiteSpace(_cfg.User) || string.IsNullOrWhiteSpace(_cfg.Password))
            {
                _log.LogInformation("Email chưa bật (Email:Enabled=false). Bỏ qua việc gửi tới {To}: {Subject}", den, tieuDe);
                return false;
            }

            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(_cfg.User, _cfg.FromName, Encoding.UTF8),
                    Subject = tieuDe,
                    Body = noiDungHtml,
                    IsBodyHtml = true,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };
                msg.To.Add(den);

                using var smtp = new SmtpClient(_cfg.Host, _cfg.Port)
                {
                    EnableSsl = _cfg.EnableSsl,
                    Credentials = new NetworkCredential(_cfg.User, _cfg.Password),
                    Timeout = 15000
                };
                await smtp.SendMailAsync(msg);
                return true;
            }
            catch (Exception ex)
            {
                // Lỗi gửi mail không được làm hỏng nghiệp vụ chính
                _log.LogWarning(ex, "Gửi email tới {To} thất bại", den);
                return false;
            }
        }

        // ===== Mẫu email =====

        public Task<bool> GuiXacNhanPhieuAsync(string? den, PhieuGuiXe p, string bienSo, string tenViTri, string linkVe, string maVe)
        {
            string H(string? s) => WebUtility.HtmlEncode(s ?? "");
            var html = Khung("Đăng ký gửi xe thành công",
                $@"<p>Xin chào <b>{H(p.PhuongTien?.ChuPhuongTien?.HoTen)}</b>,</p>
<p>Phiếu gửi xe của bạn đã được ghi nhận và đang <b>chờ xác nhận</b> tại cổng.</p>
<table style='border-collapse:collapse;width:100%;font-size:14px'>
{Dong("Mã phiếu", "#" + p.MaPhieu)}
{Dong("Biển số xe", H(bienSo))}
{Dong("Vị trí đỗ", H(tenViTri))}
{Dong("Dự kiến vào", p.ThoiGianDuKienVao.ToString("dd/MM/yyyy HH:mm"))}
{Dong("Dự kiến ra", p.ThoiGianDuKienRa.ToString("dd/MM/yyyy HH:mm"))}
{Dong("Đơn giá", p.DonGiaTheoGio.ToString("N0") + " đ/giờ")}
{Dong("Mã vé", "<code>" + H(maVe) + "</code>")}
</table>
<p style='margin-top:18px'><a href='{H(linkVe)}' style='background:#2563eb;color:#fff;padding:10px 18px;border-radius:8px;text-decoration:none'>Xem vé &amp; mã QR</a></p>
<p style='color:#64748b;font-size:12px'>Vui lòng đưa mã QR cho nhân viên quét khi vào và ra bãi. Không chia sẻ mã cho người khác.</p>");
            return GuiAsync(den, $"[ParkSmart] Xác nhận đăng ký gửi xe #{p.MaPhieu}", html);
        }

        public Task<bool> GuiXacNhanVeThangAsync(string? den, string hoTen, string bienSo, VeThang v)
        {
            string H(string? s) => WebUtility.HtmlEncode(s ?? "");
            var html = Khung("Đăng ký vé tháng thành công",
                $@"<p>Xin chào <b>{H(hoTen)}</b>,</p>
<p>Vé tháng của bạn đã được kích hoạt. Trong thời hạn vé, mỗi lượt gửi xe sẽ được <b>miễn phí</b>.</p>
<table style='border-collapse:collapse;width:100%;font-size:14px'>
{Dong("Mã vé tháng", "#" + v.MaVeThang)}
{Dong("Biển số xe", H(bienSo))}
{Dong("Thời hạn", v.NgayBatDau.ToString("dd/MM/yyyy") + " – " + v.NgayKetThuc.ToString("dd/MM/yyyy") + " (" + v.SoThang + " tháng)")}
{Dong("Phí đã đăng ký", v.GiaTien.ToString("N0") + " đ")}
</table>");
            return GuiAsync(den, $"[ParkSmart] Xác nhận vé tháng #{v.MaVeThang}", html);
        }

        private static string Dong(string nhan, string giaTri) =>
            $"<tr><td style='padding:8px 10px;border-bottom:1px solid #e5e9f2;color:#64748b;width:38%'>{nhan}</td><td style='padding:8px 10px;border-bottom:1px solid #e5e9f2'><b>{giaTri}</b></td></tr>";

        private static string Khung(string tieuDe, string noiDung) =>
            $@"<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto;border:1px solid #e5e9f2;border-radius:14px;overflow:hidden'>
<div style='background:#0b1f4d;color:#fff;padding:16px 22px'><b>ParkSmart</b><div style='font-size:18px;margin-top:2px'>{tieuDe}</div></div>
<div style='padding:20px 22px;color:#0f172a'>{noiDung}</div></div>";
    }
}