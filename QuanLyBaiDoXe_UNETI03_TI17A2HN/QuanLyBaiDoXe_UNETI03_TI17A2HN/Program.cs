using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("KNTD")
    ?? throw new InvalidOperationException("Connection string 'QuanLyBaiDoXe_UNETI03_TI17A2HNContext' not found.");
builder.Services.AddDbContext<QuanLyBaiDoXe_UNETI03_TI17A2HNContext>(options =>
    options.UseSqlServer(connectionString)
           // EF Core 9+ báo lỗi khi snapshot lệch model; bỏ qua để Migrate() vẫn áp dụng các migration đã có
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.QrVeService>();
builder.Services.AddSingleton<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.MatKhauService>();      // mã hóa mật khẩu (Identity PasswordHasher)
builder.Services.Configure<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.EmailService>();        // gửi email xác nhận
builder.Services.AddSingleton<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.XuatBaoCaoService>();   // xuất Excel / PDF
builder.Services.AddScoped<QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.AnhPhuongTienService>();   // upload ảnh phương tiện
builder.Services.AddMemoryCache();                                                              // đếm số lần đăng nhập sai
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Tự tạo / cập nhật CSDL (kèm dữ liệu mẫu) theo các migration có sẵn khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuanLyBaiDoXe_UNETI03_TI17A2HNContext>();
    try
    {
        db.Database.Migrate();
        // Đảm bảo có sơ đồ Tầng 2 (chỉ ô tô + xe khách, chia theo hàng)
        QuanLyBaiDoXe_UNETI03_TI17A2HN.Services.QuyDinhBaiXe.DamBaoTangHaiAsync(db).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        // Không để ứng dụng sập (cũng giúp các lệnh Add-Migration / Update-Database vẫn chạy được)
        app.Logger.LogError(ex, "Không tự cập nhật được CSDL. Hãy chạy Update-Database trong Package Manager Console.");
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=GioiThieu}/{id?}")
    .WithStaticAssets();
app.Run();
