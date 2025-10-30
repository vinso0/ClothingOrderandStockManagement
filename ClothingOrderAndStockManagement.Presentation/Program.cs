using ClothingOrderAndStockManagement.Application;
using ClothingOrderAndStockManagement.Infrastructure;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddApplicationDI();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
