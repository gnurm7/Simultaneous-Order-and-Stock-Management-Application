using Microsoft.EntityFrameworkCore;
using yazlab3.Controllers;
using yazlab3.Controllers.LogController;
using yazlab3.Models;

var builder = WebApplication.CreateBuilder(args);

// Servisler buraya eklenir

builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Session'ý eklemek için gerekli servisler
builder.Services.AddDistributedMemoryCache(); // Bellek tabanlý cache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout süresi
    options.Cookie.HttpOnly = true; // Güvenlik için çerezleri sadece HTTP üzerinden eriþilebilir yap
    options.Cookie.IsEssential = true; // Çerezin temel bir çerez olduðunu belirt
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Context>();



var app = builder.Build(); // Build iþlemi servisten sonra yapýlmalý

// Middleware'ler burada tanýmlanýr
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'i ekle
app.UseSession();

app.UseAuthorization();
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=OrderList}/{id?}",
    defaults: new { controller = "Admin" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
