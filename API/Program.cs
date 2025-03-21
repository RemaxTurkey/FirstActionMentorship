using API;
using API.Extensions;
using API.Filters;
using Application.Common;
using Application.Extensions;
using Application.RedisCache;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers(options => { options.Filters.Add<ApiResponseAttribute>(); });
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Logging.ConfigureLogging()
    .ConfigureEntityFrameworkLogging(false, LogLevel.Debug);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RemaxDB")));

builder.Services.AddSingleton<ICacheManager, CacheManager>();

builder.Services.RegisterInjectableServices();
builder.Services.AddCommonService();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapOpenApi();
app.MapScalarApiReference(options => { options.WithTheme(ScalarTheme.Purple); });


app.UseHttpsRedirection();
app.UseRouting();
app.UseGlobalExceptionHandler();

app.MapControllers();

app.Run();