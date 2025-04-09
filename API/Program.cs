using API;
using API.Extensions;
using API.Filters;
using API.Services;
using Application.Common;
using Application.Extensions;
using Application.RedisCache;
using Application.Services.Mail;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using RemaxSiteService.Notification;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers(options => { options.Filters.Add<ApiResponseAttribute>(); });
builder.Services.Configure<MailSettingsOptions>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddNotificationServices(builder.Configuration);

builder.Logging.ConfigureLogging()
    .ConfigureEntityFrameworkLogging(false, LogLevel.Debug);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RemaxDB")));

builder.Services.AddSingleton<ICacheManager, CacheManager>();

// BackgroundService kaydı
builder.Services.AddHostedService<FAMAcceptanceBrokerEmailNotificationJob>();

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