//Lương Thị Quỳnh Hương - 23103100064

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyBaiDoXe_UNETI03_TI17A2HN.Models;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhanQuyen : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public PhanQuyen(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var maTaiKhoan = session.GetInt32("MaTaiKhoan");
            var vaiTro = session.GetString("VaiTro");

            // 1. Kiểm tra đăng nhập
            if (!maTaiKhoan.HasValue || string.IsNullOrEmpty(vaiTro))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoans", new { returnUrl });
                return;
            }

            // 2. Kiểm tra trạng thái tài khoản từ CSDL
            var db = context.HttpContext.RequestServices.GetService<QuanLyBaiDoXe_UNETI03_TI17A2HNContext>();
            if (db != null)
            {
                // Dùng phương thức Find() để tìm trực tiếp theo Khóa chính (Primary Key) của bảng TaiKhoan
                var user = db.TaiKhoan.Find(maTaiKhoan.Value);

                if (user == null || !user.TrangThai)
                {
                    session.Clear();
                    context.Result = new RedirectToActionResult("DangNhap", "TaiKhoans", null);

                    if (context.Controller is Controller controller) //ép kiểu sang lớp controller
                    {
                        controller.TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa hoặc không còn hiệu lực.";
                    }
                    return;
                }
            }

            // 3. Kiểm tra phân quyền vai trò
            if (_roles.Length > 0 && !_roles.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);

                if (context.Controller is Controller controller)
                {
                    controller.TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này.";
                }
                return;
            }
        }
    }
}
