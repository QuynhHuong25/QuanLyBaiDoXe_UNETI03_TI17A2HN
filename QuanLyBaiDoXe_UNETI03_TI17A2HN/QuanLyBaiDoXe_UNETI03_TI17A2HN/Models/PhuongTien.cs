using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class PhuongTien
    {
        [Key]
        public int MaPhuongTien { get; set; }

        [ForeignKey("ChuPhuongTien")]
        public int MaChuPhuongTien { get; set; }

        [ForeignKey("LoaiPhuongTien")]
        public int MaLoaiPhuongTien { get; set; }

        [Required]
        [StringLength(20)]
        public string? BienSoXe { get; set; }

        [StringLength(50)]
        public string? NhanHieu { get; set; }

        [StringLength(30)]
        public string? MauSac { get; set; }

        public string? MoTa { get; set; }

        [Required]
        public bool TrangThai { get; set; } = true;

        public virtual ChuPhuongTien? ChuPhuongTien { get; set; }
        public virtual LoaiPhuongTien? LoaiPhuongTien { get; set; }
        public virtual ICollection<PhieuGuiXe>? PhieuGuiXes { get; set; }
    }
}
