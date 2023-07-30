using AutoMapper;
using FluentValidation;
using Lab1.Data;
using Lab1.Data.Entities;
using Lab1.Models;
using Lab1.Models.Dtos;
using Lab1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lab1.ApiEndpoints;

public static class CouponApiEndpoints
{
    public static RouteGroupBuilder MapCouponsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCoupons)
            .WithName("GetCoupons");

        group.MapGet("/{id:int}", GetCouponById)
            .WithName("GetCoupon");

        group.MapPost("/", CreateCoupon)
            .WithName("CreateCoupon")
            .Accepts<CouponCreateDto>("application/json")
            .Produces<ApiResponse<CouponVm>>(201)
            .Produces<ApiResponse>(400);


        group.MapPut("/{id:int}", UpdateCoupon())
            .WithName("UpdateCoupon");

        group.MapDelete("/{id}", DeleteCoupon())
            .WithName("DeleteCoupon");

        return group;
    }

    private static Func<CouponStoreDbContext, int, Task<IResult>> DeleteCoupon()
    {
        return async (dbContext, id) =>
        {
            var existingCoupon = await dbContext.Coupons.FindAsync(id);
            if (existingCoupon is null)
            {
                return Results.NotFound();
            }

            dbContext.Coupons.Remove(existingCoupon);
            await dbContext.SaveChangesAsync();

            return Results.Ok();
        };
    }

    private static Func<CouponStoreDbContext, int, Coupon, Task<IResult>> UpdateCoupon()
    {
        return async (dbContext,
            [FromRoute] id,
            [FromBody] coupon) =>
        {
            if (id != coupon.Id)
            {
                return Results.BadRequest();
            }

            var existingCoupon = await dbContext.Coupons.FindAsync(id);
            if (existingCoupon is null)
            {
                return Results.NotFound();
            }

            existingCoupon.Code = coupon.Code;
            existingCoupon.Discount = coupon.Discount;
            existingCoupon.Total = coupon.Total;
            existingCoupon.Claimed = coupon.Claimed;
            existingCoupon.IsActive = coupon.IsActive;
            existingCoupon.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Results.Ok(existingCoupon);
        };
    }

    private static async Task<Ok<ApiResponse<IReadOnlyCollection<CouponVm>>>> GetAllCoupons(
        CouponStoreDbContext dbContext, IMapper mapper)
    {
        var coupons = await dbContext.Coupons.ToListAsync();
        return TypedResults.Ok(new ApiResponse<IReadOnlyCollection<CouponVm>>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Data = mapper.Map<IReadOnlyCollection<CouponVm>>(coupons)
        });
    }

    private static async Task<Results<Ok<ApiResponse<CouponVm>>, NotFound<ApiResponse>>> GetCouponById(
        CouponStoreDbContext dbContext, IMapper mapper, [FromRoute] int id)
    {
        var coupon = await dbContext.Coupons.FindAsync(id);
        if (coupon is null)
            return TypedResults.NotFound(new ApiResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.NotFound,
            });

        return TypedResults.Ok(new ApiResponse<CouponVm>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Data = mapper.Map<CouponVm>(coupon)
        });
    }

    // TODO: replace IResult with Results<TOk, TError>
    private static async Task<IResult> CreateCoupon(
        CouponStoreDbContext dbContext,
        IMapper mapper,
        IValidator<CouponCreateDto> validator,
        [FromBody] CouponCreateDto couponCreateDto)
    {
        var validationResult = await validator.ValidateAsync(couponCreateDto);
        if (!validationResult.IsValid)
            return TypedResults.BadRequest(new ApiResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            });


        if (await dbContext.Coupons.AnyAsync(c => c.Code == couponCreateDto.Code))
            return TypedResults.BadRequest(new ApiResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new List<string> { "Coupon code already exists" }
            });

        var coupon = mapper.Map<Coupon>(couponCreateDto);
        await dbContext.Coupons.AddAsync(coupon);
        await dbContext.SaveChangesAsync();

        return TypedResults.CreatedAtRoute(new { id = coupon.Id }, "GetCoupon", new ApiResponse<CouponVm>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Data = mapper.Map<CouponVm>(coupon)
        });
    }
}