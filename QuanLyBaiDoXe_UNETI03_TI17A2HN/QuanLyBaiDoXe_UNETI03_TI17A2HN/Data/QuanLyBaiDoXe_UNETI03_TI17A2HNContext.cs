using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;


//Lương Thị Quỳnh Hương - 23103100064
public class QuanLyBaiDoXe_UNETI03_TI17A2HNContext : DbContext
{
    public QuanLyBaiDoXe_UNETI03_TI17A2HNContext(DbContextOptions<QuanLyBaiDoXe_UNETI03_TI17A2HNContext> options)
        : base(options)
    {
    }

    public DbSet<TaiKhoan> TaiKhoan { get; set; } = default!;
    public DbSet<ChuPhuongTien> ChuPhuongTien { get; set; } = default!;
    public DbSet<LoaiPhuongTien> LoaiPhuongTien { get; set; } = default!;
    public DbSet<PhuongTien> PhuongTien { get; set; } = default!;
    public DbSet<ViTriDoXe> ViTriDoXe { get; set; } = default!;
    public DbSet<PhieuGuiXe> PhieuGuiXe { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaiKhoan>().HasData(
            new TaiKhoan { MaTaiKhoan = 1, TenDangNhap = "admin", MatKhau = "123456", HoTen = "Quản Trị Viên", Email = "admin@gmail.com", VaiTro = "Admin", TrangThai = true },
            new TaiKhoan { MaTaiKhoan = 2, TenDangNhap = "huong", MatKhau = "123456", HoTen = "Lương Thị Quỳnh Hương", Email = "huong@gmail.com", VaiTro = "Khách hàng", TrangThai = true },
            new TaiKhoan { MaTaiKhoan = 3, TenDangNhap = "hay", MatKhau = "123456", HoTen = "Trịnh Thị Hay", Email = "hay@gmail.com", VaiTro = "Khách hàng", TrangThai = true },
            new TaiKhoan { MaTaiKhoan = 4, TenDangNhap = "diem", MatKhau = "123456", HoTen = "Nguyễn Thị Diễm", Email = "diem@gmail.com", VaiTro = "Khách hàng", TrangThai = true },
            new TaiKhoan { MaTaiKhoan = 5, TenDangNhap = "giang", MatKhau = "123456", HoTen = "Tăng Hoàng Giang", Email = "giang@gmail.com", VaiTro = "Khách hàng", TrangThai = true }
        );

        modelBuilder.Entity<ChuPhuongTien>().HasData(
           new ChuPhuongTien { MaChuPhuongTien = 1, MaTaiKhoan = 1, HoTen = "Quản Trị Viên", NgaySinh = new DateTime(1990, 1, 1), GioiTinh = true, SoDienThoai = "0901234567", Email = "admin@gmail.com", DiaChi = "Hà Nội", NgayDangKy = new DateTime(2026, 1, 1), TrangThai = true },
           new ChuPhuongTien { MaChuPhuongTien = 2, MaTaiKhoan = 2, HoTen = "Lương Thị Quỳnh Hương", NgaySinh = new DateTime(2005, 10, 25), GioiTinh = false, SoDienThoai = "0912345678", Email = "huong@gmail.com", DiaChi = "Hà Nội", NgayDangKy = new DateTime(2026, 1, 2), TrangThai = true },
           new ChuPhuongTien { MaChuPhuongTien = 3, MaTaiKhoan = 3, HoTen = "Trịnh Thị Hay", NgaySinh = new DateTime(1998, 8, 20), GioiTinh = false, SoDienThoai = "0923456789", Email = "hay@gmail.com", DiaChi = "Bắc Ninh", NgayDangKy = new DateTime(2026, 1, 3), TrangThai = true },
           new ChuPhuongTien { MaChuPhuongTien = 4, MaTaiKhoan = 4, HoTen = "Nguyễn Thị Diễm", NgaySinh = new DateTime(2000, 3, 15), GioiTinh = false, SoDienThoai = "0934567890", Email = "diem@gmail.com", DiaChi = "Hưng Yên", NgayDangKy = new DateTime(2026, 1, 4), TrangThai = true },
           new ChuPhuongTien { MaChuPhuongTien = 5, MaTaiKhoan = 5, HoTen = "Tăng Hoàng Giang", NgaySinh = new DateTime(2002, 11, 25), GioiTinh = true, SoDienThoai = "0945678901", Email = "giang@gmail.com", DiaChi = "Hà Nam", NgayDangKy = new DateTime(2026, 1, 5), TrangThai = true }
       );

        modelBuilder.Entity<LoaiPhuongTien>().HasData(
            new LoaiPhuongTien { MaLoaiPhuongTien = 1, TenLoaiPhuongTien = "Xe máy số", DonGiaTheoGio = 5000, MoTa = "Xe máy số các loại", TrangThai = true },
            new LoaiPhuongTien { MaLoaiPhuongTien = 2, TenLoaiPhuongTien = "Ô tô 4-7 chỗ", DonGiaTheoGio = 20000, MoTa = "Ô tô con từ 4 đến 7 chỗ", TrangThai = true },
            new LoaiPhuongTien { MaLoaiPhuongTien = 3, TenLoaiPhuongTien = "Xe đạp điện / Xe máy điện", DonGiaTheoGio = 4000, MoTa = "Xe đạp điện và xe máy điện", TrangThai = true },
            new LoaiPhuongTien { MaLoaiPhuongTien = 4, TenLoaiPhuongTien = "Xe tay ga", DonGiaTheoGio = 7000, MoTa = "Xe tay ga các loại", TrangThai = true },
            new LoaiPhuongTien { MaLoaiPhuongTien = 5, TenLoaiPhuongTien = "Ô tô trên 7 chỗ", DonGiaTheoGio = 30000, MoTa = "Ô tô khách nhỏ và xe bán tải", TrangThai = true }
        );

        modelBuilder.Entity<PhuongTien>().HasData(
            new PhuongTien { MaPhuongTien = 1, MaChuPhuongTien = 1, MaLoaiPhuongTien = 1, BienSoXe = "29A1-12345", NhanHieu = "Honda Wave", MauSac = "Đỏ", MoTa = "Xe còn mới", TrangThai = true },
            new PhuongTien { MaPhuongTien = 2, MaChuPhuongTien = 2, MaLoaiPhuongTien = 2, BienSoXe = "30F1-67890", NhanHieu = "Toyota Vios", MauSac = "Bạc", MoTa = "Ô tô 5 chỗ", TrangThai = true },
            new PhuongTien { MaPhuongTien = 3, MaChuPhuongTien = 3, MaLoaiPhuongTien = 3, BienSoXe = "99M1-55555", NhanHieu = "VinFast Feliz", MauSac = "Trắng", MoTa = "Xe máy điện", TrangThai = true },
            new PhuongTien { MaPhuongTien = 4, MaChuPhuongTien = 4, MaLoaiPhuongTien = 4, BienSoXe = "89A-11122", NhanHieu = "Honda SH", MauSac = "Đen", MoTa = "Xe chính chủ", TrangThai = true },
            new PhuongTien { MaPhuongTien = 5, MaChuPhuongTien = 5, MaLoaiPhuongTien = 5, BienSoXe = "90B-33344", NhanHieu = "Ford Transit", MauSac = "Xám", MoTa = "Ô tô 16 chỗ", TrangThai = true }
        );

        modelBuilder.Entity<ViTriDoXe>().HasData(
            new ViTriDoXe { MaViTri = 1, TenViTri = "A1-01", MaLoaiPhuongTien = 1, KhuVuc = "Khu A - Xe máy", Tang = 1, TrangThai = "Còn trống", MoTa = "Gần cổng vào" },
            new ViTriDoXe { MaViTri = 2, TenViTri = "C1-01", MaLoaiPhuongTien = 2, KhuVuc = "Khu C - Ô tô", Tang = 1, TrangThai = "Đang sử dụng", MoTa = "Vị trí rộng rãi" },
            new ViTriDoXe { MaViTri = 3, TenViTri = "B1-01", MaLoaiPhuongTien = 3, KhuVuc = "Khu B - Xe điện", Tang = 1, TrangThai = "Còn trống", MoTa = "Có trụ sạc điện" },
            new ViTriDoXe { MaViTri = 4, TenViTri = "A1-02", MaLoaiPhuongTien = 4, KhuVuc = "Khu A - Xe máy", Tang = 1, TrangThai = "Đang sử dụng", MoTa = "Cạnh lối đi" },
            new ViTriDoXe { MaViTri = 5, TenViTri = "C2-01", MaLoaiPhuongTien = 5, KhuVuc = "Khu C - Ô tô", Tang = 2, TrangThai = "Còn trống", MoTa = "Tầng 2 thoáng mát" }
        );

        modelBuilder.Entity<PhieuGuiXe>().HasData(
            new PhieuGuiXe { MaPhieu = 1, MaPhuongTien = 1, MaViTri = 1, NgayDangKy = new DateTime(2026, 9, 20), ThoiGianDuKienVao = new DateTime(2026, 9, 20, 8, 0, 0), ThoiGianDuKienRa = new DateTime(2026, 9, 20, 17, 0, 0), ThoiGianVaoThucTe = new DateTime(2026, 9, 20, 8, 5, 0), ThoiGianRaThucTe = new DateTime(2026, 9, 20, 17, 10, 0), DonGiaTheoGio = 5000, ThanhTien = 45000, TrangThai = "Đã lấy xe" },
            new PhieuGuiXe { MaPhieu = 2, MaPhuongTien = 2, MaViTri = 2, NgayDangKy = new DateTime(2026, 9, 22), ThoiGianDuKienVao = new DateTime(2026, 9, 22, 7, 30, 0), ThoiGianDuKienRa = new DateTime(2026, 9, 22, 18, 0, 0), ThoiGianVaoThucTe = new DateTime(2026, 9, 22, 7, 35, 0), ThoiGianRaThucTe = new DateTime(2026, 9, 22, 18, 0, 0), DonGiaTheoGio = 20000, ThanhTien = 210000, TrangThai = "Đã lấy xe" },
            new PhieuGuiXe { MaPhieu = 3, MaPhuongTien = 3, MaViTri = 3, NgayDangKy = new DateTime(2026, 9, 22), ThoiGianDuKienVao = new DateTime(2026, 9, 22, 9, 0, 0), ThoiGianDuKienRa = new DateTime(2026, 9, 22, 12, 0, 0), ThoiGianVaoThucTe = new DateTime(2026, 9, 22, 9, 0, 0), ThoiGianRaThucTe = new DateTime(2026, 9, 22, 12, 0, 0), DonGiaTheoGio = 4000, ThanhTien = 12000, TrangThai = "Đã lấy xe" },
            new PhieuGuiXe { MaPhieu = 4, MaPhuongTien = 4, MaViTri = 4, NgayDangKy = new DateTime(2026, 9, 22), ThoiGianDuKienVao = new DateTime(2026, 9, 22, 6, 0, 0), ThoiGianDuKienRa = new DateTime(2026, 9, 22, 20, 0, 0), ThoiGianVaoThucTe = new DateTime(2026, 9, 22, 6, 10, 0), ThoiGianRaThucTe = new DateTime(2026, 9, 22, 20, 0, 0), DonGiaTheoGio = 7000, ThanhTien = 98000, TrangThai = "Đã lấy xe" },
            new PhieuGuiXe { MaPhieu = 5, MaPhuongTien = 5, MaViTri = 5, NgayDangKy = new DateTime(2026, 9, 21), ThoiGianDuKienVao = new DateTime(2026, 9, 21, 10, 0, 0), ThoiGianDuKienRa = new DateTime(2026, 9, 21, 15, 0, 0), ThoiGianVaoThucTe = new DateTime(2026, 9, 21, 10, 0, 0), ThoiGianRaThucTe = new DateTime(2026, 9, 21, 10, 30, 0), DonGiaTheoGio = 30000, ThanhTien = 0, TrangThai = "Đã hủy" }
        );
    }
}