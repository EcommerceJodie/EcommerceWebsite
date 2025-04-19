using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Seeds;
using Ecommerce.Services.Implementations;
using Ecommerce.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Ecommerce.Shared.Storage.Minio;
using EcommerceWebsite.Middlewares;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Infrastructure.Repositories;
using FluentValidation;
using Ecommerce.Core.Validators.CategoryValidators;
using Ecommerce.Core.Validators.ProductValidators;
using Ecommerce.Core.DTOs;
using Ecommerce.Shared.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseServices(builder.Configuration);

builder.Services.AddIdentityServices();

builder.Services.AddCookieAuthentication();


Console.WriteLine("Bắt đầu cấu hình MinioService...");
builder.Services.AddMinioService(builder.Configuration);
Console.WriteLine("Đã cấu hình MinioService thành công");


builder.Services.AddRedisCache(builder.Configuration);


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IMenuConfigRepository, MenuConfigRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuConfigService, MenuConfigService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddAutoMapper(typeof(Ecommerce.Core.MapperProfiles.MappingProfile).Assembly);


builder.Services.AddCors();


builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryValidator>();
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDto>, UpdateProductValidator>();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeedData.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi seed dữ liệu Identity");
    }
}


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseCors(options =>
{
    options.SetIsOriginAllowed(_ => true)
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "vnpay-return",
    pattern: "vnpay-return",
    defaults: new { controller = "VnPay", action = "PaymentReturn" });

app.MapControllerRoute(
    name: "vnpay-ipn",
    pattern: "vnpay-ipn",
    defaults: new { controller = "VnPay", action = "PaymentIPN" });

app.Run();
