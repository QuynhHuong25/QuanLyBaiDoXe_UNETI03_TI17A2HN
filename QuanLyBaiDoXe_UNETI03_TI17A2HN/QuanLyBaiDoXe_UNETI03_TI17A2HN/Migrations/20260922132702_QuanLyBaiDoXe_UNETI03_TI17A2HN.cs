using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Migrations
{
    /// <inheritdoc />
    public partial class QuanLyBaiDoXe_UNETI03_TI17A2HN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiPhuongTien",
                columns: table => new
                {
                    MaLoaiPhuongTien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhuongTien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DonGiaTheoGio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiPhuongTien", x => x.MaLoaiPhuongTien);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "ViTriDoXe",
                columns: table => new
                {
                    MaViTri = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenViTri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoaiPhuongTien = table.Column<int>(type: "int", nullable: false),
                    KhuVuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tang = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViTriDoXe", x => x.MaViTri);
                    table.ForeignKey(
                        name: "FK_ViTriDoXe_LoaiPhuongTien_MaLoaiPhuongTien",
                        column: x => x.MaLoaiPhuongTien,
                        principalTable: "LoaiPhuongTien",
                        principalColumn: "MaLoaiPhuongTien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuPhuongTien",
                columns: table => new
                {
                    MaChuPhuongTien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<bool>(type: "bit", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuPhuongTien", x => x.MaChuPhuongTien);
                    table.ForeignKey(
                        name: "FK_ChuPhuongTien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhuongTien",
                columns: table => new
                {
                    MaPhuongTien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuPhuongTien = table.Column<int>(type: "int", nullable: false),
                    MaLoaiPhuongTien = table.Column<int>(type: "int", nullable: false),
                    BienSoXe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NhanHieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MauSac = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongTien", x => x.MaPhuongTien);
                    table.ForeignKey(
                        name: "FK_PhuongTien_ChuPhuongTien_MaChuPhuongTien",
                        column: x => x.MaChuPhuongTien,
                        principalTable: "ChuPhuongTien",
                        principalColumn: "MaChuPhuongTien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhuongTien_LoaiPhuongTien_MaLoaiPhuongTien",
                        column: x => x.MaLoaiPhuongTien,
                        principalTable: "LoaiPhuongTien",
                        principalColumn: "MaLoaiPhuongTien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuGuiXe",
                columns: table => new
                {
                    MaPhieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhuongTien = table.Column<int>(type: "int", nullable: false),
                    MaViTri = table.Column<int>(type: "int", nullable: true),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianDuKienVao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianDuKienRa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianVaoThucTe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianRaThucTe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonGiaTheoGio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuGuiXe", x => x.MaPhieu);
                    table.ForeignKey(
                        name: "FK_PhieuGuiXe_PhuongTien_MaPhuongTien",
                        column: x => x.MaPhuongTien,
                        principalTable: "PhuongTien",
                        principalColumn: "MaPhuongTien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuGuiXe_ViTriDoXe_MaViTri",
                        column: x => x.MaViTri,
                        principalTable: "ViTriDoXe",
                        principalColumn: "MaViTri");
                });

            migrationBuilder.InsertData(
                table: "LoaiPhuongTien",
                columns: new[] { "MaLoaiPhuongTien", "DonGiaTheoGio", "MoTa", "TenLoaiPhuongTien", "TrangThai" },
                values: new object[,]
                {
                    { 1, 5000m, "Xe máy số các loại", "Xe máy số", true },
                    { 2, 20000m, "Ô tô con từ 4 đến 7 chỗ", "Ô tô 4-7 chỗ", true },
                    { 3, 4000m, "Xe đạp điện và xe máy điện", "Xe đạp điện / Xe máy điện", true },
                    { 4, 7000m, "Xe tay ga các loại", "Xe tay ga", true },
                    { 5, 30000m, "Ô tô khách nhỏ và xe bán tải", "Ô tô trên 7 chỗ", true }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "MaTaiKhoan", "Email", "HoTen", "MatKhau", "TenDangNhap", "TrangThai", "VaiTro" },
                values: new object[,]
                {
                    { 1, "admin@gmail.com", "Quản Trị Viên", "123456", "admin", true, "Admin" },
                    { 2, "huong@gmail.com", "Lương Thị Quỳnh Hương", "123456", "huong", true, "Khách hàng" },
                    { 3, "hay@gmail.com", "Trịnh Thị Hay", "123456", "hay", true, "Khách hàng" },
                    { 4, "diem@gmail.com", "Nguyễn Thị Diễm", "123456", "diem", true, "Khách hàng" },
                    { 5, "giang@gmail.com", "Tăng Hoàng Giang", "123456", "giang", true, "Khách hàng" }
                });

            migrationBuilder.InsertData(
                table: "ChuPhuongTien",
                columns: new[] { "MaChuPhuongTien", "DiaChi", "Email", "GioiTinh", "HoTen", "MaTaiKhoan", "NgayDangKy", "NgaySinh", "SoDienThoai", "TrangThai" },
                values: new object[,]
                {
                    { 1, "Hà Nội", "admin@gmail.com", true, "Quản Trị Viên", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "0901234567", true },
                    { 2, "Hà Nội", "huong@gmail.com", false, "Lương Thị Quỳnh Hương", 2, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2005, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "0912345678", true },
                    { 3, "Bắc Ninh", "hay@gmail.com", false, "Trịnh Thị Hay", 3, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1998, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0923456789", true },
                    { 4, "Hưng Yên", "diem@gmail.com", false, "Nguyễn Thị Diễm", 4, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2000, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "0934567890", true },
                    { 5, "Hà Nam", "giang@gmail.com", true, "Tăng Hoàng Giang", 5, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2002, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "0945678901", true }
                });

            migrationBuilder.InsertData(
                table: "ViTriDoXe",
                columns: new[] { "MaViTri", "KhuVuc", "MaLoaiPhuongTien", "MoTa", "Tang", "TenViTri", "TrangThai" },
                values: new object[,]
                {
                    { 1, "Khu A - Xe máy", 1, "Gần cổng vào", 1, "A1-01", "Còn trống" },
                    { 2, "Khu C - Ô tô", 2, "Vị trí rộng rãi", 1, "C1-01", "Đang sử dụng" },
                    { 3, "Khu B - Xe điện", 3, "Có trụ sạc điện", 1, "B1-01", "Còn trống" },
                    { 4, "Khu A - Xe máy", 4, "Cạnh lối đi", 1, "A1-02", "Đang sử dụng" },
                    { 5, "Khu C - Ô tô", 5, "Tầng 2 thoáng mát", 2, "C2-01", "Còn trống" }
                });

            migrationBuilder.InsertData(
                table: "PhuongTien",
                columns: new[] { "MaPhuongTien", "BienSoXe", "MaChuPhuongTien", "MaLoaiPhuongTien", "MauSac", "MoTa", "NhanHieu", "TrangThai" },
                values: new object[,]
                {
                    { 1, "29A1-12345", 1, 1, "Đỏ", "Xe còn mới", "Honda Wave", true },
                    { 2, "30F1-67890", 2, 2, "Bạc", "Ô tô 5 chỗ", "Toyota Vios", true },
                    { 3, "99M1-55555", 3, 3, "Trắng", "Xe máy điện", "VinFast Feliz", true },
                    { 4, "89A-11122", 4, 4, "Đen", "Xe chính chủ", "Honda SH", true },
                    { 5, "90B-33344", 5, 5, "Xám", "Ô tô 16 chỗ", "Ford Transit", true }
                });

            migrationBuilder.InsertData(
                table: "PhieuGuiXe",
                columns: new[] { "MaPhieu", "DonGiaTheoGio", "MaPhuongTien", "MaViTri", "NgayDangKy", "ThanhTien", "ThoiGianDuKienRa", "ThoiGianDuKienVao", "ThoiGianRaThucTe", "ThoiGianVaoThucTe", "TrangThai" },
                values: new object[,]
                {
                    { 1, 5000m, 1, 1, new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000m, new DateTime(2026, 9, 20, 17, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 20, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 20, 17, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 20, 8, 5, 0, 0, DateTimeKind.Unspecified), "Đã lấy xe" },
                    { 2, 20000m, 2, 2, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 22, 18, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 22, 7, 30, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 22, 7, 35, 0, 0, DateTimeKind.Unspecified), "Đang gửi" },
                    { 3, 4000m, 3, 3, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Chờ xác nhận" },
                    { 4, 7000m, 4, 4, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 22, 20, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 22, 6, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 9, 22, 6, 10, 0, 0, DateTimeKind.Unspecified), "Đang gửi" },
                    { 5, 30000m, 5, 5, new DateTime(2026, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 0m, new DateTime(2026, 9, 21, 15, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 21, 10, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Đã hủy" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuPhuongTien_MaTaiKhoan",
                table: "ChuPhuongTien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuGuiXe_MaPhuongTien",
                table: "PhieuGuiXe",
                column: "MaPhuongTien");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuGuiXe_MaViTri",
                table: "PhieuGuiXe",
                column: "MaViTri");

            migrationBuilder.CreateIndex(
                name: "IX_PhuongTien_MaChuPhuongTien",
                table: "PhuongTien",
                column: "MaChuPhuongTien");

            migrationBuilder.CreateIndex(
                name: "IX_PhuongTien_MaLoaiPhuongTien",
                table: "PhuongTien",
                column: "MaLoaiPhuongTien");

            migrationBuilder.CreateIndex(
                name: "IX_ViTriDoXe_MaLoaiPhuongTien",
                table: "ViTriDoXe",
                column: "MaLoaiPhuongTien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhieuGuiXe");

            migrationBuilder.DropTable(
                name: "PhuongTien");

            migrationBuilder.DropTable(
                name: "ViTriDoXe");

            migrationBuilder.DropTable(
                name: "ChuPhuongTien");

            migrationBuilder.DropTable(
                name: "LoaiPhuongTien");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
