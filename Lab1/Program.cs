using FluentValidation;
using Lab1.ApiEndpoints;
using Lab1.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CouponStoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CouponDB")));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api/coupons")
    .MapCouponsApi()
    .WithTags("Coupon API");

app.UseHttpsRedirection();
app.Run();