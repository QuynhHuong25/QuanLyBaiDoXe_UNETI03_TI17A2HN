using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class ChuPhuongTien
    {
        [Key]
        public int MaChuPhuongTien { get; set; }

        [ForeignKey("TaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string? HoTen { get; set; }

        [Required]
        public DateTime? NgaySinh { get; set; }

        [Required]
        public bool GioiTinh { get; set; }

        [Required]
        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [Required]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? DiaChi { get; set; }

        [Required]
        public DateTime? NgayDangKy { get; set; } = DateTime.Now;

        [Required]
        public bool TrangThai { get; set; } = true;

        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<PhuongTien>? PhuongTiens { get; set; }
    }
}
