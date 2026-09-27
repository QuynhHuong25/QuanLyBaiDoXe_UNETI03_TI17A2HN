using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class ViTriDoXe
    {
        [Key]
        public int MaViTri { get; set; }

        [Required]
        [StringLength(50)]
        public string? TenViTri { get; set; }

        [ForeignKey("LoaiPhuongTien")]
        public int MaLoaiPhuongTien { get; set; }

        [Required]
        [StringLength(50)]
        public string? KhuVuc { get; set; }

        [Required]
        public int Tang { get; set; } = 1;

        [Required]
        [StringLength(50)]
        public string? TrangThai { get; set; } = "Còn trống"; // "Còn trống", "Đang sử dụng", "Tạm ngừng"

        public string? MoTa { get; set; }

        public virtual LoaiPhuongTien? LoaiPhuongTien { get; set; }
        public virtual ICollection<PhieuGuiXe>? PhieuGuiXes { get; set; }
    }
}
