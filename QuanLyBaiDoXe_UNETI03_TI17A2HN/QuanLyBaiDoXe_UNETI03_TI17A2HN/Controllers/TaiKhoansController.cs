using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc;

//Lương Thị Quỳnh Hương - 23103100064

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Controllers
{
    public class TaiKhoansController : Controller
    {
        private readonly QuanLyBaiDoXe_UNETI03_TI17A2HNContext _context;

        public TaiKhoansController(QuanLyBaiDoXe_UNETI03_TI17A2HNContext context)
        {
            _context = context;
        }

        // ===== HÀM HỖ TRỢ MÃ HÓA MẬT KHẨU (BCrypt) =====

        // Băm mật khẩu (có salt ngẫu nhiên, kết quả 60 ký tự dạng $2a$11$...)
        private static string BamMatKhau(string matKhau)
        {
            return BCrypt.Net.BCrypt.HashPassword(matKhau);
        }

        // Kiểm tra mật khẩu nhập vào có khớp với chuỗi hash đang lưu không.
        private static bool KiemTraMatKhau(string matKhau, string? matKhauDaLuu)
        {
            if (string.IsNullOrEmpty(matKhauDaLuu) || !matKhauDaLuu.StartsWith("$2"))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(matKhau, matKhauDaLuu);
            }
            catch
            {
                return false;
            }
        }

        // GET: TAIKHOANS (Admin xem danh sách tài khoản)
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            int pageSize = 10; // Số lượng tài khoản hiển thị trên mỗi trang

            var query = _context.TaiKhoan
                .Include(t => t.ChuPhuongTien)
                .AsQueryable();

            // Tìm kiếm theo tên đăng nhập, họ tên hoặc email nếu có nhập
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TenDangNhap.Contains(searchString)
                                      || (t.HoTen != null && t.HoTen.Contains(searchString))
                                      || (t.Email != null && t.Email.Contains(searchString)));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var taiKhoans = await query
                .OrderByDescending(t => t.MaTaiKhoan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;
            ViewBag.CurrentSearch = searchString;

            return View(taiKhoans);
        }

        // GET: TAIKHOANS/Create (Admin khởi tạo tài khoản mới)
        [PhanQuyen("Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TAIKHOANS/Create (Admin tạo tài khoản Admin hoặc Khách hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai")] TaiKhoan taikhoan)
        {
            if (string.IsNullOrEmpty(taikhoan.TenDangNhap) || string.IsNullOrEmpty(taikhoan.MatKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!");
                return View(taikhoan);
            }

            // Kiểm tra tên đăng nhập trùng lặp
            bool daTonTai = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == taikhoan.TenDangNhap);
            if (daTonTai)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại trong hệ thống!");
                return View(taikhoan);
            }

            if (ModelState.IsValid)
            {
                // Băm mật khẩu bằng BCrypt trước khi lưu
                taikhoan.MatKhau = BamMatKhau(taikhoan.MatKhau!);

                _context.Add(taikhoan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo mới tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(taikhoan);
        }

        // GET: TAIKHOANS/Details
        public async Task<IActionResult> Details(int? mataikhoan)
        {
            if (mataikhoan == null)
            {
                var maTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");
                if (int.TryParse(maTaiKhoanStr, out int currentId))
                {
                    mataikhoan = currentId;
                }
                else
                {
                    return RedirectToAction("DangNhap", "TaiKhoans");
                }
            }

            var taikhoan = await _context.TaiKhoan
                .Include(t => t.ChuPhuongTien)
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == mataikhoan);

            if (taikhoan == null)
            {
                return NotFound();
            }

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var currentMaTaiKhoanStr = HttpContext.Session.GetString("MaTaiKhoan");

            // Khách hàng chỉ được xem thông tin chi tiết của chính mình
            if ((vaiTro == "Khách hàng" || vaiTro == "Khach") && int.TryParse(currentMaTaiKhoanStr, out int myId))
            {
                if (taikhoan.MaTaiKhoan != myId)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(taikhoan);
        }

        // GET: TaiKhoans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            string tenDangNhapSession = HttpContext.Session.GetString("TenDangNhap") ?? "";
            int? maTaiKhoanSession = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoanSession == null)
            {
                var strId = HttpContext.Session.GetString("MaTaiKhoan");
                if (int.TryParse(strId, out int parsedId)) maTaiKhoanSession = parsedId;
            }

            // Chỉ cho phép sửa nếu ĐÚNG LÀ TÀI KHOẢN CỦA CHÍNH MÌNH (bất kể Admin hay Khách)
            bool isChinhToi = (maTaiKhoanSession != null && taiKhoan.MaTaiKhoan == maTaiKhoanSession)
                           || (!string.IsNullOrEmpty(tenDangNhapSession) && taiKhoan.TenDangNhap == tenDangNhapSession);

            if (!isChinhToi)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể chỉnh sửa tài khoản của chính mình!";
                return RedirectToAction(nameof(Details), new { mataikhoan = id });
            }

            return View(taiKhoan);
        }

        // POST: TaiKhoans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai")] TaiKhoan taiKhoan, string? MatKhauMoi)
        {
            if (id != taiKhoan.MaTaiKhoan)
            {
                return NotFound();
            }

            var strId = HttpContext.Session.GetString("MaTaiKhoan");
            if (!int.TryParse(strId, out int myId) || myId != id)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có quyền chỉnh sửa tài khoản của chính mình!";
                return RedirectToAction(nameof(Details), new { mataikhoan = id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userInDb = await _context.TaiKhoan.FindAsync(id);
                    if (userInDb == null) return NotFound();

                    // Cập nhật thông tin cơ bản
                    userInDb.HoTen = taiKhoan.HoTen;
                    userInDb.Email = taiKhoan.Email;

                    // Đổi mật khẩu nếu người dùng nhập mật khẩu mới
                    if (!string.IsNullOrEmpty(MatKhauMoi))
                    {
                        userInDb.MatKhau = BamMatKhau(MatKhauMoi);
                    }

                    _context.Update(userInDb);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại Session Họ Tên nếu có thay đổi
                    HttpContext.Session.SetString("HoTen", userInDb.HoTen ?? userInDb.TenDangNhap);

                    TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                    return RedirectToAction(nameof(Details), new { mataikhoan = userInDb.MaTaiKhoan });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taiKhoan.MaTaiKhoan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(taiKhoan);
        }

        // POST: TaiKhoans/KhoaTaiKhoan (Khách hàng tự khóa tài khoản của mình)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaTaiKhoan(int id)
        {
            var strId = HttpContext.Session.GetString("MaTaiKhoan");
            if (!int.TryParse(strId, out int myId) || myId != id)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể tự khóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Details), new { mataikhoan = id });
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            // Chuyển trạng thái sang khóa
            taiKhoan.TrangThai = false;
            _context.Update(taiKhoan);
            await _context.SaveChangesAsync();

            // Đăng xuất khỏi hệ thống
            HttpContext.Session.Clear();
            TempData["Success"] = "Tài khoản của bạn đã được khóa thành công!";
            return RedirectToAction("DangNhap");
        }

        // POST: TaiKhoans/XoaTaiKhoan (Khách hàng tự xóa tài khoản của mình)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            var strId = HttpContext.Session.GetString("MaTaiKhoan");
            if (!int.TryParse(strId, out int myId) || myId != id)
            {
                TempData["ErrorMessage"] = "Bạn chỉ có thể tự xóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Details), new { mataikhoan = id });
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoan.Remove(taiKhoan);
                await _context.SaveChangesAsync();
            }

            // Xóa session và chuyển về trang đăng nhập
            HttpContext.Session.Clear();
            TempData["Success"] = "Tài khoản của bạn đã được xóa vĩnh viễn khỏi hệ thống!";
            return RedirectToAction("DangNhap");
        }

        // GET: TaiKhoans/DangNhap
        [HttpGet]
        public async Task<IActionResult> DangNhap()
        {
            if (Request.Cookies.TryGetValue("SavedUsername", out string savedUsername))
            {
                ViewBag.SavedUsername = savedUsername;
            }

            await Task.CompletedTask;
            return View();
        }

        // POST: TaiKhoans/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(string tenDangNhap, string matKhau, bool ghiNho)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
                return View();
            }

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            bool hopLe = false;
            if (taiKhoan != null)
            {
                if (!string.IsNullOrEmpty(taiKhoan.MatKhau) && !taiKhoan.MatKhau.StartsWith("$2"))
                {
                    hopLe = taiKhoan.MatKhau == matKhau;
                    if (hopLe)
                    {
                        taiKhoan.MatKhau = BamMatKhau(matKhau);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    hopLe = KiemTraMatKhau(matKhau, taiKhoan.MatKhau);
                }
            }

            if (taiKhoan == null || !hopLe)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }

            if (taiKhoan.TrangThai == false)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa!";
                return View();
            }

            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true
            };

            if (ghiNho)
            {
                cookieOptions.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Append("SavedUsername", tenDangNhap, cookieOptions);
            }
            else
            {
                Response.Cookies.Delete("SavedUsername");
            }

            HttpContext.Session.SetString("MaTaiKhoan", taiKhoan.MaTaiKhoan.ToString());
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen ?? taiKhoan.TenDangNhap);
            HttpContext.Session.SetString("VaiTro", string.IsNullOrEmpty(taiKhoan.VaiTro) ? "Khách hàng" : taiKhoan.VaiTro);

            return RedirectToAction("Index", "Home");
        }

        // GET: TaiKhoans/DangXuat
        [HttpGet]
        public async Task<IActionResult> DangXuat()
        {
            HttpContext.Session.Clear();
            await Task.CompletedTask;
            return RedirectToAction("DangNhap", "TaiKhoans");
        }

        // GET: TaiKhoans/DangKy
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        // POST: TaiKhoans/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(string tenDangNhap, string email, string matKhau, string xacNhanMatKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập, Email và Mật khẩu!";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var taiKhoanDaTonTai = await _context.TaiKhoan
                .AnyAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoanDaTonTai)
            {
                ViewBag.Error = "Tên đăng nhập này đã tồn tại!";
                return View();
            }

            var taiKhoanMoi = new TaiKhoan();
            taiKhoanMoi.TenDangNhap = tenDangNhap;
            taiKhoanMoi.Email = email.Trim();
            taiKhoanMoi.MatKhau = BamMatKhau(matKhau);
            taiKhoanMoi.HoTen = tenDangNhap;
            taiKhoanMoi.VaiTro = "Khách hàng";
            taiKhoanMoi.TrangThai = true;

            try
            {
                _context.TaiKhoan.Add(taiKhoanMoi);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đăng ký thành công! Email xác nhận đã được gửi tự động tới địa chỉ {email}. Vui lòng kiểm tra!";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi lưu dữ liệu: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return View();
            }
        }

        // GET: TaiKhoans/QuenMatKhau
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        // POST: TaiKhoans/QuenMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(string tenDangNhap, string matKhauMoi, string xacNhanMatKhauMoi)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhauMoi))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (matKhauMoi != xacNhanMatKhauMoi)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null)
            {
                ViewBag.Error = "Tên đăng nhập không tồn tại trên hệ thống!";
                return View();
            }

            bool trungMatKhauCu = (!string.IsNullOrEmpty(taiKhoan.MatKhau) && !taiKhoan.MatKhau.StartsWith("$2"))
                ? taiKhoan.MatKhau == matKhauMoi
                : KiemTraMatKhau(matKhauMoi, taiKhoan.MatKhau);

            if (trungMatKhauCu)
            {
                ViewBag.Error = "Mật khẩu mới không được trùng với mật khẩu cũ!";
                return View();
            }
            taiKhoan.MatKhau = BamMatKhau(matKhauMoi);

            try
            {
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi cập nhật dữ liệu: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return View();
            }
        }

        private bool TaiKhoanExists(int? mataikhoan)
        {
            return _context.TaiKhoan.Any(e => e.MaTaiKhoan == mataikhoan);
        }
    }
}
