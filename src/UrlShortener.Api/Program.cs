using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using UrlShortener.Api.Services;
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure;
using UrlShortener.Core.DTOs;
// Thêm các namespace thiếu
using UrlShortener.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "URL Shortener API", Version = "v1" });
    
    // Thêm mô tả cho endpoint Redirect
    c.DocInclusionPredicate((docName, apiDesc) => true);
    
    // Thêm ví dụ cho request body
    c.MapType<UrlShortenRequest>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["longUrl"] = new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("https://example.com")
            },
            ["customCode"] = new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("example")
            }
        },
        Required = new HashSet<string> { "longUrl" }
    });
});

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add application services
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Configure options
builder.Services.Configure<UrlShortenerSettings>(builder.Configuration.GetSection("UrlShortener"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Thêm health check endpoints
builder.Services.AddHealthChecks();

var app = builder.Build();

// Thêm đoạn code này để tự động migrate database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<UrlDbContext>();
        // Đảm bảo database được tạo
        context.Database.EnsureCreated();
        // Nếu có migrations thì có thể dùng:
        // context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Đảm bảo logger đã được đăng ký
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener API v1");
        c.RoutePrefix = "swagger";
    });
}

// Bỏ dòng app.UseHttpsRedirection(); để tránh chuyển hướng HTTPS
app.UseCors("AllowAll");
app.UseAuthorization();

// Thêm endpoint health check
app.MapHealthChecks("/api/health");
app.MapControllers();

app.Run();