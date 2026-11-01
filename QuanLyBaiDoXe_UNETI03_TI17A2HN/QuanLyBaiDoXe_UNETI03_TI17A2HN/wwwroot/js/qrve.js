// Vẽ mã QR lên <canvas> (dùng thư viện qrcode-generator, chạy hoàn toàn trên trình duyệt)
window.qrVe = (function () {
    function ve(canvas, text, kichThuoc) {
        var qr = qrcode(0, 'M');           // tự chọn phiên bản, mức sửa lỗi M
        qr.addData(text);
        qr.make();

        var n = qr.getModuleCount();
        var vien = 4;                      // vùng trống 4 ô quanh QR (chuẩn để quét dễ)
        var o = Math.max(2, Math.ceil((kichThuoc || 300) / (n + vien * 2)));
        var size = (n + vien * 2) * o;

        canvas.width = size;
        canvas.height = size;

        var ctx = canvas.getContext('2d');
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, size, size);
        ctx.fillStyle = '#000000';
        for (var r = 0; r < n; r++) {
            for (var c = 0; c < n; c++) {
                if (qr.isDark(r, c)) ctx.fillRect((c + vien) * o, (r + vien) * o, o, o);
            }
        }
    }

    function taiAnh(canvas, tenFile) {
        canvas.toBlob(function (blob) {
            var a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = tenFile || 'qr-ve-gui-xe.png';
            document.body.appendChild(a);
            a.click();
            setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 500);
        }, 'image/png');
    }

    return { ve: ve, taiAnh: taiAnh };
})();
