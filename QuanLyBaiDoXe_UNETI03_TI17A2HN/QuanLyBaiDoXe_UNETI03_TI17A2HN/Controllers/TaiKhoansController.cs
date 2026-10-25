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

        // GET: TAIKHOANS
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            int pageSize = 10; // Số lượng tài khoản hiển thị trên mỗi trang

            var query = _context.TaiKhoan.Include(t => t.ChuPhuongTien).AsQueryable();

            // Tìm kiếm theo tên đăng nhập, họ tên hoặc email nếu có nhập
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TenDangNhap.Contains(searchString)
                                      || (t.HoTen != null && t.HoTen.Contains(searchString))
                                      || (t.Email != null && t.Email.Contains(searchString)));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo trang hiện tại nằm trong giới hạn hợp lệ
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var taiKhoans = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu trữ thông tin phục vụ hiển thị ở View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages > 0 ? totalPages : 1;
            ViewBag.CurrentSearch = searchString;

            return View(taiKhoans);
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
                    return RedirectToAction("DangNhap", "TaiKhoan");
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
            if (vaiTro == "Khách hàng" && int.TryParse(currentMaTaiKhoanStr, out int myId))
            {
                if (taikhoan.MaTaiKhoan != myId)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(taikhoan);
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
        public async Task<IActionResult> Create([Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai,ChuPhuongTien")] TaiKhoan taikhoan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taikhoan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            // Lấy thông tin đăng nhập từ Session
            string vaiTro = HttpContext.Session.GetString("VaiTro") ?? "";
            string tenDangNhapSession = HttpContext.Session.GetString("TenDangNhap") ?? "";
            int? maTaiKhoanSession = HttpContext.Session.GetInt32("MaTaiKhoan");

            // Kiểm tra quyền: Chỉ chính tài khoản đó hoặc Admin mới được phép chỉnh sửa
            bool isChinhToi = (maTaiKhoanSession != null && taiKhoan.MaTaiKhoan == maTaiKhoanSession)
                        || (!string.IsNullOrEmpty(tenDangNhapSession) && taiKhoan.TenDangNhap == tenDangNhapSession)
                        || (vaiTro == "Admin");

            if (!isChinhToi)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản này!";
                return RedirectToAction(nameof(Details), new { id = id });
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

            if (ModelState.IsValid)
            {
                try
                {
                    var userInDb = await _context.TaiKhoan.FindAsync(id);
                    if (userInDb == null) return NotFound();

                    // Cập nhật các thông tin cơ bản
                    userInDb.HoTen = taiKhoan.HoTen;
                    userInDb.Email = taiKhoan.Email;

                    // Đổi mật khẩu nếu người dùng nhập mật khẩu mới
                    if (!string.IsNullOrEmpty(MatKhauMoi))
                    {
                        userInDb.MatKhau = MatKhauMoi;
                    }

                    _context.Update(userInDb);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại Session Họ Tên nếu có thay đổi
                    HttpContext.Session.SetString("HoTen", userInDb.HoTen ?? userInDb.TenDangNhap);

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
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

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoan.Any(e => e.MaTaiKhoan == id);
        }

        // GET: TAIKHOANS/Delete/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Delete(int? mataikhoan)
        {
            if (mataikhoan == null)
            {
                return NotFound();
            }

            var taikhoan = await _context.TaiKhoan
                .Include(t => t.ChuPhuongTien)
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == mataikhoan);
            if (taikhoan == null)
            {
                return NotFound();
            }

            return View(taikhoan);
        }

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
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap && t.MatKhau == matKhau);

            if (taiKhoan == null)
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

        [HttpGet]
        public async Task<IActionResult> DangXuat()
        {
            HttpContext.Session.Clear();
            await Task.CompletedTask;
            return RedirectToAction("DangNhap", "TaiKhoans");
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(string tenDangNhap, string email, string matKhau, string xacNhanMatKhau)
        {
            // Kiểm tra không thiếu thông tin (Tên đăng nhập, Email, Mật khẩu)
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập, Email và Mật khẩu!";
                return View();
            }

            // Mật khẩu xác nhận phải khớp
            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // Kiểm tra tên đăng nhập chưa tồn tại (dùng AnyAsync)
            var taiKhoanDaTonTai = await _context.TaiKhoan
                .AnyAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoanDaTonTai)
            {
                ViewBag.Error = "Tên đăng nhập này đã tồn tại!";
                return View();
            }

            // Tạo tài khoản mới: vai trò Khách hàng, trạng thái hoạt động (true), họ tên lấy theo tên đăng nhập
            var taiKhoanMoi = new TaiKhoan();
            taiKhoanMoi.TenDangNhap = tenDangNhap;
            taiKhoanMoi.Email = email.Trim();
            taiKhoanMoi.MatKhau = matKhau;
            taiKhoanMoi.HoTen = tenDangNhap;
            taiKhoanMoi.VaiTro = "Khách hàng";
            taiKhoanMoi.TrangThai = true;

            try
            {
                _context.TaiKhoan.Add(taiKhoanMoi);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi lưu dữ liệu: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return View();
            }
        }

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

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

            if (taiKhoan.MatKhau == matKhauMoi)
            {
                ViewBag.Error = "Mật khẩu mới không được trùng với mật khẩu cũ!";
                return View();
            }
            taiKhoan.MatKhau = matKhauMoi;

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

        // POST: TAIKHOANS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int? mataikhoan)
        {
            var taikhoan = await _context.TaiKhoan.FindAsync(mataikhoan);
            if (taikhoan != null)
            {
                _context.TaiKhoan.Remove(taikhoan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int? mataikhoan)
        {
            return _context.TaiKhoan.Any(e => e.MaTaiKhoan == mataikhoan);
        }
    }
}
