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
        public async Task<IActionResult> Index()
        {
            var quanLyBaiDoXe_UNETI03_TI17A2HNContext = _context.TaiKhoan.Include(t => t.ChuPhuongTien);
            return View(await quanLyBaiDoXe_UNETI03_TI17A2HNContext.ToListAsync());
        }

        // GET: TAIKHOANS/Details/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Details(int? mataikhoan)
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

        // GET: TAIKHOANS/Edit/5
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? mataikhoan)
        {
            if (mataikhoan == null)
            {
                return NotFound();
            }

            var taikhoan = await _context.TaiKhoan.FindAsync(mataikhoan);
            if (taikhoan == null)
            {
                return NotFound();
            }
            return View(taikhoan);
        }

        // POST: TAIKHOANS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen("Admin")]
        public async Task<IActionResult> Edit(int? mataikhoan, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,Email,VaiTro,TrangThai,ChuPhuongTien")] TaiKhoan taikhoan)
        {
            if (mataikhoan != taikhoan.MaTaiKhoan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taikhoan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taikhoan.MaTaiKhoan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taikhoan);
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
        public async Task<IActionResult> DangKy(string tenDangNhap, string matKhau, string xacNhanMatKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
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
            taiKhoanMoi.MatKhau = matKhau;
            taiKhoanMoi.HoTen = tenDangNhap;
            taiKhoanMoi.Email = $"{tenDangNhap.ToLower().Trim()}@gmail.com";
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
