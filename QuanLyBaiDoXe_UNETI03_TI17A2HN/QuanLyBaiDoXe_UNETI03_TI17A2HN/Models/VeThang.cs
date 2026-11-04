using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    /// <summary>Vé tháng: xe có vé còn hạn được gửi xe miễn phí trong thời hạn vé.</summary>
    public class VeThang
    {
        [Key]
        public int MaVeThang { get; set; }

        [ForeignKey("PhuongTien")]
        public int MaPhuongTien { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        [Range(1, 24)]
        public int SoThang { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTien { get; set; }

        /// <summary>"Hoạt động" hoặc "Đã hủy". Hết hạn được tính theo ngày, không lưu.</summary>
        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = "Hoạt động";

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? GhiChu { get; set; }

        public virtual PhuongTien? PhuongTien { get; set; }

        /// <summary>Trạng thái hiển thị: Đã hủy / Chưa bắt đầu / Đang hiệu lực / Hết hạn</summary>
        [NotMapped]
        public string TrangThaiHienThi =>
            TrangThai == "Đã hủy" ? "Đã hủy"
            : NgayKetThuc.Date < DateTime.Today ? "Hết hạn"
            : NgayBatDau.Date > DateTime.Today ? "Chưa bắt đầu"
            : "Đang hiệu lực";

        [NotMapped]
        public bool DangHieuLuc => TrangThaiHienThi == "Đang hiệu lực";
    }
}
