using Ecommerce.API.Middlewares;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Validators.CategoryValidators;
using Ecommerce.Core.Validators.ProductValidators;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using Ecommerce.Services.Implementations;
using Ecommerce.Services.Interfaces;
using Ecommerce.Shared.Storage.Minio;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var corsSettings = builder.Configuration.GetSection("CorsSettings");
        
        if (corsSettings.GetValue<bool>("AllowAnyOrigin"))
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? 
                    new string[] { "http://localhost:3000" })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    });
});

builder.Services.AddDatabaseServices(builder.Configuration);

builder.Services.AddIdentityServices();

builder.Services.AddJwtAuthentication(builder.Configuration);


Console.WriteLine("Bắt đầu cấu hình MinioService...");
var minioConfig = builder.Configuration.GetSection("MinioConfig").Get<Ecommerce.Shared.Storage.Minio.Configs.MinioConfig>();
if (minioConfig == null)
{
    Console.WriteLine("Lỗi: Không thể đọc cấu hình MinioConfig từ appsettings. Vui lòng kiểm tra lại.");
    throw new InvalidOperationException("Không thể đọc cấu hình MinioConfig từ appsettings. Vui lòng kiểm tra lại.");
}

Console.WriteLine($"Đã đọc cấu hình MinIO: Endpoint={minioConfig.Endpoint}, AccessKey={minioConfig.AccessKey}, BucketName={minioConfig.BucketName}");
Console.WriteLine("Đang gọi AddMinioService...");
builder.Services.AddMinioService(builder.Configuration);
Console.WriteLine("Đã cấu hình MinioService thành công");

builder.Services.AddScoped<Ecommerce.Core.Interfaces.Services.ITokenService, Ecommerce.Services.Implementations.TokenService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ecommerce API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IMenuConfigRepository, MenuConfigRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IMenuConfigService, MenuConfigService>();


builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryValidator>();
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDto>, UpdateProductValidator>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ProductDto).Assembly);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddRedisCache(builder.Configuration);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();


app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();


app.UseTransactionMiddleware();

app.MapControllers().RequireCors("AllowSpecificOrigins");

app.Run();
