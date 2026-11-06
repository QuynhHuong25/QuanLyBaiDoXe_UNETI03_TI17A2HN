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

        private static string BamMatKhau(string matKhau)
        {
            return BCrypt.Net.BCrypt.HashPassword(matKhau);
        }

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
            int pageSize = 10;

            var query = _context.TaiKhoan
                .Include(t => t.ChuPhuongTien)
                .AsQueryable();

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
                .OrderBy(t => t.MaTaiKhoan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;
            ViewBag.CurrentSearch = searchString;

            return View(taiKhoans);
        }

        // GET: TAIKHOANS/Create
        [PhanQuyen("Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TAIKHOANS/Create
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

            bool daTonTai = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == taikhoan.TenDangNhap);
            if (daTonTai)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã tồn tại trong hệ thống!");
                return View(taikhoan);
            }

            if (ModelState.IsValid)
            {
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

            // Nếu là Khách hàng (không phải Admin), chỉ được xem thông tin chính mình
            if (vaiTro != "Admin" && int.TryParse(currentMaTaiKhoanStr, out int myId))
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

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            int? maTaiKhoanSession = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoanSession == null)
            {
                var strId = HttpContext.Session.GetString("MaTaiKhoan");
                if (int.TryParse(strId, out int parsedId)) maTaiKhoanSession = parsedId;
            }

            bool isAdmin = (vaiTro == "Admin");
            bool isChinhToi = (maTaiKhoanSession != null && taiKhoan.MaTaiKhoan == maTaiKhoanSession);

            if (!isAdmin && !isChinhToi)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản này!";
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

            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var strId = HttpContext.Session.GetString("MaTaiKhoan");
            int.TryParse(strId, out int myId);

            bool isAdmin = (vaiTro == "Admin");
            bool isChinhToi = (myId == id);

            if (!isAdmin && !isChinhToi)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản này!";
                return RedirectToAction(nameof(Details), new { mataikhoan = id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userInDb = await _context.TaiKhoan.FindAsync(id);
                    if (userInDb == null) return NotFound();

                    // KIỂM TRA TRÙNG LẶP TÊN ĐĂNG NHẬP (nếu người dùng có đổi tên mới)
                    string tenMoi = taiKhoan.TenDangNhap?.Trim() ?? "";
                    if (string.IsNullOrEmpty(tenMoi))
                    {
                        ModelState.AddModelError("TenDangNhap", "Tên đăng nhập không được để trống!");
                        return View(taiKhoan);
                    }

                    if (tenMoi != userInDb.TenDangNhap)
                    {
                        bool daTonTai = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == tenMoi && t.MaTaiKhoan != id);
                        if (daTonTai)
                        {
                            ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã được sử dụng bởi tài khoản khác!");
                            return View(taiKhoan);
                        }
                        userInDb.TenDangNhap = tenMoi;
                    }

                    userInDb.HoTen = taiKhoan.HoTen;
                    userInDb.Email = taiKhoan.Email;

                    if (isAdmin)
                    {
                        userInDb.VaiTro = taiKhoan.VaiTro;
                        userInDb.TrangThai = taiKhoan.TrangThai;
                    }

                    if (!string.IsNullOrEmpty(MatKhauMoi))
                    {
                        userInDb.MatKhau = BamMatKhau(MatKhauMoi);
                    }

                    _context.Update(userInDb);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại Session nếu chính chủ tự đổi thông tin của mình
                    if (isChinhToi)
                    {
                        HttpContext.Session.SetString("TenDangNhap", userInDb.TenDangNhap);
                        HttpContext.Session.SetString("HoTen", userInDb.HoTen ?? userInDb.TenDangNhap);
                    }

                    TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                    return RedirectToAction(nameof(Details), new { id = userInDb.MaTaiKhoan });
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

        // GET: TaiKhoans/DangNhap
        [HttpGet]
        public async Task<IActionResult> DangNhap()
        {
            if (Request.Cookies.TryGetValue("SavedUsername", out string? savedUsername))
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

            var taiKhoanDaTonTai = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == tenDangNhap);
            if (taiKhoanDaTonTai)
            {
                ViewBag.Error = "Tên đăng nhập này đã tồn tại!";
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var taiKhoanMoi = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    Email = email.Trim(),
                    MatKhau = BamMatKhau(matKhau),
                    HoTen = tenDangNhap,
                    VaiTro = "Khách hàng",
                    TrangThai = true
                };

                _context.TaiKhoan.Add(taiKhoanMoi);
                await _context.SaveChangesAsync();

                var chuPhuongTienMoi = new ChuPhuongTien
                {
                    MaTaiKhoan = taiKhoanMoi.MaTaiKhoan,
                    HoTen = tenDangNhap,
                    Email = email.Trim(),
                    NgaySinh = new DateTime(2000, 1, 1),
                    GioiTinh = true,
                    SoDienThoai = "Chưa cập nhật",
                    DiaChi = "Chưa cập nhật",
                    NgayDangKy = DateTime.Now,
                    TrangThai = true
                };

                _context.ChuPhuongTien.Add(chuPhuongTienMoi);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ViewBag.Error = "Lỗi hệ thống khi đăng ký: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
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
            if (mataikhoan == null) return false;
            return _context.TaiKhoan.Any(e => e.MaTaiKhoan == mataikhoan);
        }
    }
}
