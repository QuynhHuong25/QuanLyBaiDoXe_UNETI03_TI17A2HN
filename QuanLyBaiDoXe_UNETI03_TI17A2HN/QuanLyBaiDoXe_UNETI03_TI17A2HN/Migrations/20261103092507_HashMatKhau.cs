using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Migrations
{
    /// <inheritdoc />
    public partial class HashMatKhau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 2,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "TrangThai" },
                values: new object[] { 210000m, new DateTime(2026, 9, 22, 18, 0, 0, 0, DateTimeKind.Unspecified), "Đã lấy xe" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 3,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "ThoiGianVaoThucTe", "TrangThai" },
                values: new object[] { 12000m, new DateTime(2026, 9, 22, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "Đã lấy xe" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 4,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "TrangThai" },
                values: new object[] { 98000m, new DateTime(2026, 9, 22, 20, 0, 0, 0, DateTimeKind.Unspecified), "Đã lấy xe" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 5,
                columns: new[] { "ThoiGianRaThucTe", "ThoiGianVaoThucTe" },
                values: new object[] { new DateTime(2026, 9, 21, 10, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 21, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 1,
                column: "MatKhau",
                value: "$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 2,
                column: "MatKhau",
                value: "$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 3,
                column: "MatKhau",
                value: "$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 4,
                column: "MatKhau",
                value: "$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 5,
                column: "MatKhau",
                value: "$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO");
            migrationBuilder.Sql(@"
                UPDATE TaiKhoan
                SET MatKhau = '$2a$11$BEP17I3E0q9D.9iEQ16ZvOzfJOER5ju5Km7wNf49pOTEZgp4Pm6VO'
                WHERE MatKhau NOT LIKE '$2%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 2,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "TrangThai" },
                values: new object[] { null, null, "Đang gửi" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 3,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "ThoiGianVaoThucTe", "TrangThai" },
                values: new object[] { null, null, null, "Chờ xác nhận" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 4,
                columns: new[] { "ThanhTien", "ThoiGianRaThucTe", "TrangThai" },
                values: new object[] { null, null, "Đang gửi" });

            migrationBuilder.UpdateData(
                table: "PhieuGuiXe",
                keyColumn: "MaPhieu",
                keyValue: 5,
                columns: new[] { "ThoiGianRaThucTe", "ThoiGianVaoThucTe" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 1,
                column: "MatKhau",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 2,
                column: "MatKhau",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 3,
                column: "MatKhau",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 4,
                column: "MatKhau",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "MaTaiKhoan",
                keyValue: 5,
                column: "MatKhau",
                value: "123456");
        }
    }
}
