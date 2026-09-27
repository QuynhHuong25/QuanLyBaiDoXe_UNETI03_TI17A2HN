using System.ComponentModel.DataAnnotations;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(50)]
        public string? TenDangNhap { get; set; }

        [Required]
        [StringLength(100)]
        public string? MatKhau { get; set; }

        [Required]
        [StringLength(100)]
        public string? HoTen { get; set; }

        [Required]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(20)]
        public string? VaiTro { get; set; } = "Khách hàng"; // "Admin" hoặc "Khách hàng"

        [Required]
        public bool TrangThai { get; set; } = true;

        public virtual ChuPhuongTien? ChuPhuongTien { get; set; }
    }
}
