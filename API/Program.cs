using API;
using API.Extensions;
using API.Filters;
using Application.Extensions;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseAttribute>();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RemaxDB")));

builder.Services.RegisterInjectableServices();
builder.Services.AddCommonService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseGlobalExceptionHandler();

app.MapControllers();

app.Run();