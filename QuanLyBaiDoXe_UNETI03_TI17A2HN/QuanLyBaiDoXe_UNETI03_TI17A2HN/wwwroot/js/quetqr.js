// Trang quét QR tại cổng: đọc mã bằng camera (jsQR) hoặc ô nhập tay / máy quét mã vạch
(function () {
    var app = document.getElementById('qrApp');
    if (!app) return;

    var urlTraCuu = app.dataset.urlTracuu;
    var urlXacNhan = app.dataset.urlXacnhan;
    var token = app.querySelector('input[name="__RequestVerificationToken"]').value;

    var el = function (id) { return document.getElementById(id); };
    var video = el('qsVideo'), canvas = el('qsCanvas'), ctx = canvas.getContext('2d', { willReadFrequently: true });
    var btnCam = el('qsBtnCam'), camList = el('qsCamList'), camErr = el('qsCamErr'), idle = el('qsIdle');
    var chkAuto = el('qsAuto'), input = el('qsInput');
    var empty = el('qsEmpty'), body = el('qsBody'), log = el('qsLog');

    var stream = null, rafId = 0, dangXuLy = false, maHienTai = null;
    var maCuoi = '', thoiDiemCuoi = 0;

    try { chkAuto.checked = localStorage.getItem('qsAuto') === '1'; } catch (e) { }
    chkAuto.addEventListener('change', function () {
        try { localStorage.setItem('qsAuto', chkAuto.checked ? '1' : '0'); } catch (e) { }
    });

    // ---------- tiện ích ----------
    function h(tag, cls, text) {
        var n = document.createElement(tag);
        if (cls) n.className = cls;
        if (text !== undefined && text !== null) n.textContent = text;   // textContent: không chèn HTML từ dữ liệu
        return n;
    }

    function bip(ok) {
        try {
            var a = new (window.AudioContext || window.webkitAudioContext)();
            var o = a.createOscillator(), g = a.createGain();
            o.frequency.value = ok ? 880 : 220; g.gain.value = 0.08;
            o.connect(g); g.connect(a.destination); o.start();
            setTimeout(function () { o.stop(); a.close(); }, ok ? 120 : 280);
        } catch (e) { }
    }

    function post(url, ma) {
        var f = new URLSearchParams();
        f.append('ma', ma);
        f.append('__RequestVerificationToken', token);
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: f.toString(),
            credentials: 'same-origin'
        }).then(function (r) {
            if (r.redirected || r.status === 401 || r.status === 400) throw new Error('Phiên đăng nhập đã hết hạn hoặc không có quyền. Hãy tải lại trang.');
            return r.json();
        });
    }

    function themLog(ok, text) {
        if (log.firstElementChild && log.firstElementChild.tagName === 'SPAN') log.innerHTML = '';
        var d = h('div', 'qs-log-i ' + (ok ? 'ok' : 'er'));
        d.appendChild(h('span', 't', new Date().toLocaleTimeString('vi-VN')));
        d.appendChild(h('span', 'm', text));
        log.insertBefore(d, log.firstChild);
        while (log.children.length > 6) log.removeChild(log.lastChild);
    }

    // ---------- hiển thị kết quả ----------
    function hang(nhan, giaTri) {
        var d = h('div');
        d.appendChild(h('small', null, nhan));
        d.appendChild(h('b', null, giaTri || '—'));
        return d;
    }

    function veKetQua(kq, ma) {
        empty.hidden = true; body.hidden = false; body.innerHTML = '';
        var p = kq.phieu;

        var banner = h('div', 'qs-banner ' + (kq.ok ? (kq.hanhDong === 'vao' ? 'vao' : 'ra') : 'er'));
        banner.appendChild(h('b', null, kq.ok ? (kq.hanhDong === 'vao' ? 'XE VÀO BÃI' : 'XE RA KHỎI BÃI') : 'KHÔNG HỢP LỆ'));
        banner.appendChild(h('span', null, kq.message));
        body.appendChild(banner);

        if (p) {
            body.appendChild(h('div', 'ps-pl qs-plate', p.bienSo));
            var g = h('div', 'qs-info');
            g.appendChild(hang('Loại xe', p.loaiXe));
            g.appendChild(hang('Chủ xe', p.chuXe));
            g.appendChild(hang('Vị trí đỗ', p.viTri));
            g.appendChild(hang('Khu / Tầng', p.khuVuc ? p.khuVuc + ' · Tầng ' + p.tang : '—'));
            g.appendChild(hang('Dự kiến vào', p.duKienVao));
            g.appendChild(hang('Dự kiến ra', p.duKienRa));
            if (p.vaoThucTe) g.appendChild(hang('Vào thực tế', p.vaoThucTe));
            g.appendChild(hang('Đơn giá', p.donGia + ' đ/giờ'));
            body.appendChild(g);

            if (kq.ok && kq.hanhDong === 'ra' && p.tamTinhTien) {
                var t = h('div', 'qs-fee');
                t.appendChild(h('span', null, 'Tạm tính đến lúc này (' + p.tamTinhGio + ' giờ)'));
                t.appendChild(h('b', null, p.tamTinhTien + ' VNĐ'));
                body.appendChild(t);
            }
        }

        var act = h('div', 'd-flex gap-2 mt-3 flex-wrap');
        if (kq.ok) {
            var bt = h('button', 'btn btn-primary qs-confirm', kq.hanhDong === 'vao' ? '✓ Cho xe vào' : '✓ Cho xe ra và thu tiền');
            bt.type = 'button';
            bt.addEventListener('click', function () { xacNhan(ma, bt); });
            act.appendChild(bt);
        }
        var bo = h('button', 'btn btn-outline-secondary', kq.ok ? 'Hủy' : 'Quét mã khác');
        bo.type = 'button';
        bo.addEventListener('click', resetKetQua);
        act.appendChild(bo);
        body.appendChild(act);
    }

    function veThongBao(ok, msg) {
        empty.hidden = true; body.hidden = false; body.innerHTML = '';
        var banner = h('div', 'qs-banner ' + (ok ? 'done' : 'er'));
        banner.appendChild(h('b', null, ok ? 'THÀNH CÔNG' : 'LỖI'));
        banner.appendChild(h('span', null, msg));
        body.appendChild(banner);
        var bt = h('button', 'btn btn-primary mt-3', 'Quét xe tiếp theo');
        bt.type = 'button';
        bt.addEventListener('click', resetKetQua);
        body.appendChild(bt);
    }

    function resetKetQua() {
        body.hidden = true; body.innerHTML = ''; empty.hidden = false;
        maHienTai = null; dangXuLy = false; input.value = ''; maCuoi = '';
    }

    // ---------- gọi server ----------
    function xuLyMa(ma) {
        ma = (ma || '').trim();
        if (!ma || dangXuLy) return;
        dangXuLy = true; maHienTai = ma;

        post(urlTraCuu, ma).then(function (kq) {
            bip(kq.ok);
            veKetQua(kq, ma);
            themLog(kq.ok, (kq.phieu && kq.phieu.bienSo ? kq.phieu.bienSo + ' – ' : '') + kq.message);
            if (kq.ok && chkAuto.checked) xacNhan(ma, null);
        }).catch(function (e) {
            bip(false); veThongBao(false, e.message || 'Không kết nối được máy chủ.');
            dangXuLy = false;
        });
    }

    function xacNhan(ma, btn) {
        if (btn) { btn.disabled = true; btn.textContent = 'Đang xử lý...'; }
        post(urlXacNhan, ma).then(function (kq) {
            bip(kq.ok);
            veThongBao(kq.ok, kq.message);
            themLog(kq.ok, kq.message);
            dangXuLy = false;
            if (kq.ok) setTimeout(function () { if (maHienTai === ma && !body.hidden) resetKetQua(); }, 6000);
        }).catch(function (e) {
            veThongBao(false, e.message || 'Không kết nối được máy chủ.');
            dangXuLy = false;
        });
    }

    // ---------- nhập tay / máy quét mã vạch (gõ mã + Enter) ----------
    el('qsManual').addEventListener('submit', function (e) {
        e.preventDefault();
        dangXuLy = false;
        xuLyMa(input.value);
    });

    // ---------- camera ----------
    function dungCamera() {
        cancelAnimationFrame(rafId);
        if (stream) stream.getTracks().forEach(function (t) { t.stop(); });
        stream = null; video.srcObject = null;
        idle.hidden = false;
        btnCam.innerHTML = '<i class="bi bi-camera me-1"></i> Bật camera';
    }

    function batCamera(deviceId) {
        camErr.hidden = true;
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            camErr.hidden = false;
            camErr.textContent = 'Trình duyệt không cho dùng camera (cần mở trang bằng HTTPS hoặc localhost). Bạn vẫn có thể nhập mã ở ô bên dưới.';
            return;
        }
        if (stream) dungCamera();

        var rangBuoc = deviceId ? { deviceId: { exact: deviceId } } : { facingMode: { ideal: 'environment' } };
        navigator.mediaDevices.getUserMedia({ video: rangBuoc, audio: false }).then(function (s) {
            stream = s; video.srcObject = s;
            return video.play();
        }).then(function () {
            idle.hidden = true;
            btnCam.innerHTML = '<i class="bi bi-camera-video-off me-1"></i> Tắt camera';
            return navigator.mediaDevices.enumerateDevices();
        }).then(function (ds) {
            var cams = ds.filter(function (d) { return d.kind === 'videoinput'; });
            if (cams.length > 1 && camList.options.length === 0) {
                cams.forEach(function (c, i) {
                    var o = document.createElement('option');
                    o.value = c.deviceId; o.textContent = c.label || ('Camera ' + (i + 1));
                    camList.appendChild(o);
                });
                camList.hidden = false;
            }
            quet();
        }).catch(function (e) {
            camErr.hidden = false;
            camErr.textContent = 'Không mở được camera: ' + (e && e.message ? e.message : e) + '. Hãy cấp quyền camera hoặc nhập mã thủ công.';
            dungCamera();
        });
    }

    var lanCuoi = 0;
    function quet(ts) {
        rafId = requestAnimationFrame(quet);
        if (!stream || video.readyState < 2 || dangXuLy) return;
        if (ts && ts - lanCuoi < 120) return;     // ~8 lần/giây là đủ
        lanCuoi = ts || 0;

        var w = video.videoWidth, hh = video.videoHeight;
        if (!w) return;
        var tile = Math.min(1, 640 / w);
        canvas.width = Math.round(w * tile); canvas.height = Math.round(hh * tile);
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

        var img = ctx.getImageData(0, 0, canvas.width, canvas.height);
        var kq = jsQR(img.data, img.width, img.height, { inversionAttempts: 'dontInvert' });
        if (kq && kq.data) {
            var now = Date.now();
            if (kq.data === maCuoi && now - thoiDiemCuoi < 3000) return;   // tránh quét lặp cùng mã
            maCuoi = kq.data; thoiDiemCuoi = now;
            xuLyMa(kq.data);
        }
    }

    btnCam.addEventListener('click', function () { stream ? dungCamera() : batCamera(camList.value || null); });
    camList.addEventListener('change', function () { batCamera(camList.value); });
    window.addEventListener('beforeunload', dungCamera);
})();
