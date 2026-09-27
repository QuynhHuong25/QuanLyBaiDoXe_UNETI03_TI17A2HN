using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Models
{
    public class LoaiPhuongTien
    {
        [Key]
        public int MaLoaiPhuongTien { get; set; }

        [Required]
        [StringLength(100)]
        public string? TenLoaiPhuongTien { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaTheoGio { get; set; }

        public string? MoTa { get; set; }

        [Required]
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<PhuongTien>? PhuongTiens { get; set; }
        public virtual ICollection<ViTriDoXe>? ViTriDoXes { get; set; }
    }
}
