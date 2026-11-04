using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Migrations
{
    /// <summary>
    /// Tính năng nâng cao: thêm cột PhuongTien.HinhAnh (upload ảnh phương tiện)
    /// và bảng VeThang (quản lý vé tháng).
    /// </summary>
    [DbContext(typeof(QuanLyBaiDoXe_UNETI03_TI17A2HNContext))]
    [Migration("20261103100000_ThemVeThangVaAnhPhuongTien")]
    public partial class ThemVeThangVaAnhPhuongTien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "PhuongTien",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VeThang",
                columns: table => new
                {
                    MaVeThang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhuongTien = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoThang = table.Column<int>(type: "int", nullable: false),
                    GiaTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeThang", x => x.MaVeThang);
                    table.ForeignKey(
                        name: "FK_VeThang_PhuongTien_MaPhuongTien",
                        column: x => x.MaPhuongTien,
                        principalTable: "PhuongTien",
                        principalColumn: "MaPhuongTien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VeThang_MaPhuongTien",
                table: "VeThang",
                column: "MaPhuongTien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "VeThang");
            migrationBuilder.DropColumn(name: "HinhAnh", table: "PhuongTien");
        }
    }
}
