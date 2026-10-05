using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyBaiDoXe_UNETI03_TI17A2HN.Lọc
{
    public class PhanQuyen : Attribute, IActionFilter
    {
        private readonly string[] _roles;

        public PhanQuyen(params string[] roles)
        {
            _roles = roles;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var vaiTro = session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoans", null);
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}