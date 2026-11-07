using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models.ViewModels;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Services
{
    /// <summary>
    /// Xuất báo cáo Excel (ClosedXML) và PDF (QuestPDF).
    /// Cần gói NuGet: ClosedXML, QuestPDF
    /// </summary>
    public class XuatBaoCaoService
    {
        static XuatBaoCaoService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ================= PHIẾU GỬI XE =================
        public byte[] ExcelPhieu(List<PhieuGuiXe> ds)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Phiếu gửi xe");

            string[] tieuDe = { "Mã phiếu", "Biển số", "Chủ xe", "Loại xe", "Vị trí", "Ngày đăng ký",
                                "Dự kiến vào", "Dự kiến ra", "Vào thực tế", "Ra thực tế", "Đơn giá/giờ", "Thành tiền", "Trạng thái" };
            for (int i = 0; i < tieuDe.Length; i++)
            {
                var c = ws.Cell(1, i + 1);
                c.Value = tieuDe[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }

            int r = 2;
            foreach (var p in ds)
            {
                ws.Cell(r, 1).Value = p.MaPhieu;
                ws.Cell(r, 2).Value = p.PhuongTien?.BienSoXe ?? "";
                ws.Cell(r, 3).Value = p.PhuongTien?.ChuPhuongTien?.HoTen ?? "";
                ws.Cell(r, 4).Value = p.PhuongTien?.LoaiPhuongTien?.TenLoaiPhuongTien ?? "";
                ws.Cell(r, 5).Value = p.ViTriDoXe?.TenViTri ?? "";
                ws.Cell(r, 6).Value = p.NgayDangKy;
                ws.Cell(r, 7).Value = p.ThoiGianDuKienVao;
                ws.Cell(r, 8).Value = p.ThoiGianDuKienRa;
                if (p.ThoiGianVaoThucTe.HasValue) ws.Cell(r, 9).Value = p.ThoiGianVaoThucTe.Value;
                if (p.ThoiGianRaThucTe.HasValue) ws.Cell(r, 10).Value = p.ThoiGianRaThucTe.Value;
                ws.Cell(r, 11).Value = p.DonGiaTheoGio;
                ws.Cell(r, 12).Value = p.ThanhTien ?? 0;
                ws.Cell(r, 13).Value = p.TrangThai ?? "";
                r++;
            }

            foreach (var col in new[] { 6, 7, 8, 9, 10 }) ws.Column(col).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            foreach (var col in new[] { 11, 12 }) ws.Column(col).Style.NumberFormat.Format = "#,##0";

            // Dòng tổng doanh thu
            ws.Cell(r + 1, 11).Value = "Tổng thu:";
            ws.Cell(r + 1, 11).Style.Font.Bold = true;
            ws.Cell(r + 1, 12).Value = ds.Where(x => x.TrangThai == "Đã lấy xe").Sum(x => x.ThanhTien ?? 0);
            ws.Cell(r + 1, 12).Style.Font.Bold = true;
            ws.Cell(r + 1, 12).Style.NumberFormat.Format = "#,##0";

            ws.SheetView.FreezeRows(1);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] PdfPhieu(List<PhieuGuiXe> ds, string tieuDe)
        {
            decimal tong = ds.Where(x => x.TrangThai == "Đã lấy xe").Sum(x => x.ThanhTien ?? 0);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("ParkSmart – " + tieuDe).FontSize(16).Bold().FontColor("#0B1F4D");
                        col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Số phiếu: {ds.Count}   |   Tổng thu (đã lấy xe): {tong:N0} đ")
                            .FontSize(9).FontColor("#64748B");
                        col.Item().PaddingBottom(6);
                    });

                    page.Content().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(34);   // Mã
                            c.RelativeColumn(1.4f); // Biển số
                            c.RelativeColumn(2f);   // Chủ xe
                            c.RelativeColumn(1.8f); // Loại xe
                            c.RelativeColumn(1f);   // Vị trí
                            c.RelativeColumn(1.7f); // Dự kiến vào
                            c.RelativeColumn(1.7f); // Dự kiến ra
                            c.RelativeColumn(1.2f); // Thành tiền
                            c.RelativeColumn(1.4f); // Trạng thái
                        });

                        string[] hd = { "Mã", "Biển số", "Chủ xe", "Loại xe", "Vị trí", "Dự kiến vào", "Dự kiến ra", "Thành tiền", "Trạng thái" };
                        t.Header(h =>
                        {
                            foreach (var s in hd)
                                h.Cell().Background("#0B1F4D").Padding(4).Text(s).FontColor("#FFFFFF").Bold();
                        });

                        bool chan = false;
                        foreach (var p in ds)
                        {
                            string nen = chan ? "#F4F6FB" : "#FFFFFF";
                            chan = !chan;
                            void O(string? v) => t.Cell().Background(nen).BorderBottom(0.5f).BorderColor("#E5E9F2").Padding(4).Text(v ?? "");

                            O("#" + p.MaPhieu);
                            O(p.PhuongTien?.BienSoXe);
                            O(p.PhuongTien?.ChuPhuongTien?.HoTen);
                            O(p.PhuongTien?.LoaiPhuongTien?.TenLoaiPhuongTien);
                            O(p.ViTriDoXe?.TenViTri);
                            O(p.ThoiGianDuKienVao.ToString("dd/MM/yy HH:mm"));
                            O(p.ThoiGianDuKienRa.ToString("dd/MM/yy HH:mm"));
                            O((p.ThanhTien ?? 0).ToString("N0"));
                            O(p.TrangThai);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ").FontColor("#64748B");
                        x.CurrentPageNumber().FontColor("#64748B");
                        x.Span(" / ").FontColor("#64748B");
                        x.TotalPages().FontColor("#64748B");
                    });
                });
            }).GeneratePdf();
        }

        // ================= THỐNG KÊ =================
        // Họ và tên: Tăng Hoàng Giang – MSV: 23103100031
        // Xuất Excel tổng hợp dữ liệu Thống kê (Module 4 – Chức năng nâng cao)
        public byte[] ExcelThongKe(ThongKeViewModel vm, int nam)
        {
            using var wb = new XLWorkbook();

            // ---- Sheet 1: Doanh thu & lượt gửi theo tháng ----
            var ws1 = wb.Worksheets.Add("Doanh thu theo tháng");
            string[] hd1 = { "Tháng", "Năm", "Số lượt gửi", "Doanh thu (đ)" };
            for (int i = 0; i < hd1.Length; i++)
            {
                var c = ws1.Cell(1, i + 1);
                c.Value = hd1[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r1 = 2;
            foreach (var x in vm.LuotGuiVaDoanhThuTheoThang.OrderBy(x => x.Thang))
            {
                ws1.Cell(r1, 1).Value = x.Thang;
                ws1.Cell(r1, 2).Value = x.Nam;
                ws1.Cell(r1, 3).Value = x.SoLuot;
                ws1.Cell(r1, 4).Value = x.DoanhThu;
                r1++;
            }
            // Dòng tổng
            ws1.Cell(r1 + 1, 2).Value = "Tổng:";
            ws1.Cell(r1 + 1, 2).Style.Font.Bold = true;
            ws1.Cell(r1 + 1, 3).Value = vm.LuotGuiVaDoanhThuTheoThang.Sum(x => x.SoLuot);
            ws1.Cell(r1 + 1, 3).Style.Font.Bold = true;
            ws1.Cell(r1 + 1, 4).Value = vm.LuotGuiVaDoanhThuTheoThang.Sum(x => x.DoanhThu);
            ws1.Cell(r1 + 1, 4).Style.Font.Bold = true;
            ws1.Column(4).Style.NumberFormat.Format = "#,##0";
            ws1.SheetView.FreezeRows(1);
            ws1.Columns().AdjustToContents();

            // ---- Sheet 2: Lượt gửi theo loại xe ----
            var ws2 = wb.Worksheets.Add("Lượt gửi theo loại xe");
            string[] hd2 = { "Loại xe", "Số lượt gửi" };
            for (int i = 0; i < hd2.Length; i++)
            {
                var c = ws2.Cell(1, i + 1);
                c.Value = hd2[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r2 = 2;
            foreach (var x in vm.LuotGuiTheoLoaiXe)
            {
                ws2.Cell(r2, 1).Value = x.TenLoai;
                ws2.Cell(r2, 2).Value = x.SoLuong;
                r2++;
            }
            ws2.SheetView.FreezeRows(1);
            ws2.Columns().AdjustToContents();

            // ---- Sheet 3: Số phương tiện theo loại ----
            var ws3 = wb.Worksheets.Add("Phương tiện theo loại");
            string[] hd3 = { "Loại xe", "Số phương tiện" };
            for (int i = 0; i < hd3.Length; i++)
            {
                var c = ws3.Cell(1, i + 1);
                c.Value = hd3[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r3 = 2;
            foreach (var x in vm.SoPhuongTienTheoLoai)
            {
                ws3.Cell(r3, 1).Value = x.TenLoai;
                ws3.Cell(r3, 2).Value = x.SoLuong;
                r3++;
            }
            ws3.SheetView.FreezeRows(1);
            ws3.Columns().AdjustToContents();

            // ---- Sheet 4: Lượt gửi theo vị trí ----
            var ws4 = wb.Worksheets.Add("Lượt gửi theo vị trí");
            string[] hd4 = { "Vị trí", "Số lượt gửi" };
            for (int i = 0; i < hd4.Length; i++)
            {
                var c = ws4.Cell(1, i + 1);
                c.Value = hd4[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r4 = 2;
            foreach (var x in vm.LuotGuiTheoViTri)
            {
                ws4.Cell(r4, 1).Value = x.TenViTri;
                ws4.Cell(r4, 2).Value = x.SoLuot;
                r4++;
            }
            ws4.SheetView.FreezeRows(1);
            ws4.Columns().AdjustToContents();

            // ---- Sheet 5: Lượt gửi theo chủ xe ----
            var ws5 = wb.Worksheets.Add("Lượt gửi theo chủ xe");
            string[] hd5 = { "Họ tên chủ xe", "Số lượt gửi" };
            for (int i = 0; i < hd5.Length; i++)
            {
                var c = ws5.Cell(1, i + 1);
                c.Value = hd5[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r5 = 2;
            foreach (var x in vm.LuotGuiTheoChuXe)
            {
                ws5.Cell(r5, 1).Value = x.HoTen;
                ws5.Cell(r5, 2).Value = x.SoLuot;
                r5++;
            }
            ws5.SheetView.FreezeRows(1);
            ws5.Columns().AdjustToContents();

            // ---- Sheet 6: Tỷ lệ lấp đầy theo tầng ----
            var ws6 = wb.Worksheets.Add("Tỷ lệ lấp đầy");
            string[] hd6 = { "Tầng", "Tổng vị trí", "Vị trí đang dùng", "Tỷ lệ (%)" };
            for (int i = 0; i < hd6.Length; i++)
            {
                var c = ws6.Cell(1, i + 1);
                c.Value = hd6[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }
            int r6 = 2;
            foreach (var x in vm.TyLeLapDauTheoTang)
            {
                ws6.Cell(r6, 1).Value = x.TenTang;
                ws6.Cell(r6, 2).Value = x.TongSoViTri;
                ws6.Cell(r6, 3).Value = x.SoViTriSuDung;
                ws6.Cell(r6, 4).Value = x.TyLe;
                r6++;
            }
            ws6.SheetView.FreezeRows(1);
            ws6.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ================= VÉ THÁNG =================
        public byte[] ExcelVeThang(List<VeThang> ds)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Vé tháng");

            string[] tieuDe = { "Mã vé", "Biển số", "Chủ xe", "Loại xe", "Bắt đầu", "Kết thúc", "Số tháng", "Phí", "Trạng thái", "Ngày đăng ký" };
            for (int i = 0; i < tieuDe.Length; i++)
            {
                var c = ws.Cell(1, i + 1);
                c.Value = tieuDe[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B1F4D");
                c.Style.Font.FontColor = XLColor.White;
            }

            int r = 2;
            foreach (var v in ds)
            {
                ws.Cell(r, 1).Value = v.MaVeThang;
                ws.Cell(r, 2).Value = v.PhuongTien?.BienSoXe ?? "";
                ws.Cell(r, 3).Value = v.PhuongTien?.ChuPhuongTien?.HoTen ?? "";
                ws.Cell(r, 4).Value = v.PhuongTien?.LoaiPhuongTien?.TenLoaiPhuongTien ?? "";
                ws.Cell(r, 5).Value = v.NgayBatDau;
                ws.Cell(r, 6).Value = v.NgayKetThuc;
                ws.Cell(r, 7).Value = v.SoThang;
                ws.Cell(r, 8).Value = v.GiaTien;
                ws.Cell(r, 9).Value = v.TrangThaiHienThi;
                ws.Cell(r, 10).Value = v.NgayDangKy;
                r++;
            }
            ws.Column(5).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(6).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(10).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            ws.Column(8).Style.NumberFormat.Format = "#,##0";
            ws.SheetView.FreezeRows(1);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
