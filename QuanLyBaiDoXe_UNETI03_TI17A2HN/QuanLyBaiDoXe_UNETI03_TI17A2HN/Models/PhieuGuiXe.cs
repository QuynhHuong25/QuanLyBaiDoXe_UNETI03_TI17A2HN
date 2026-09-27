using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class PhieuGuiXe
    {
        [Key]
        public int MaPhieu { get; set; }

        [ForeignKey("PhuongTien")]
        public int MaPhuongTien { get; set; }

        // Đổi int thành int? để EF Core tự động bỏ CASCADE DELETE
        [ForeignKey("ViTriDoXe")]
        public int? MaViTri { get; set; }

        [Required]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Required]
        public DateTime ThoiGianDuKienVao { get; set; }

        [Required]
        public DateTime ThoiGianDuKienRa { get; set; }

        public DateTime? ThoiGianVaoThucTe { get; set; }

        public DateTime? ThoiGianRaThucTe { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaTheoGio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ThanhTien { get; set; }

        [Required]
        [StringLength(50)]
        public string? TrangThai { get; set; } = "Chờ xác nhận";

        public virtual PhuongTien? PhuongTien { get; set; }
        public virtual ViTriDoXe? ViTriDoXe { get; set; }
    }
}