using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UrlShortener.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Đăng ký ApiSettings từ section "Api" trong appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));

// Cấu hình HttpClient với thêm logging
builder.Services.AddHttpClient("ApiClient", client => {
    // Thêm timeout mặc định
    client.Timeout = TimeSpan.FromSeconds(30);
    
    // Thêm Default Headers nếu cần
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();