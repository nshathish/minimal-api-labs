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

namespace Lab1.ApiEndpoints;

public static class CouponApiEndpoints
{
    public static RouteGroupBuilder MapCouponsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCoupons())
            .WithName("GetCoupons")
            .Produces<ApiResponse<IReadOnlyCollection<CouponVm>>>();

        group.MapGet("/{id:int}", GetCouponById())
            .WithName("GetCoupon")
            .Produces<ApiResponse<CouponVm>>()
            .Produces<ApiResponse>(404);

        group.MapPost("/", CreateCoupon())
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

    private static Func<CouponStoreDbContext, IMapper, IValidator<CouponCreateDto>, CouponCreateDto, Task<IResult>>
        CreateCoupon()
    {
        return async (dbContext, mapper, validator,
            [FromBody] cuoCreateDto) =>
        {
            var validationResult = await validator.ValidateAsync(cuoCreateDto);
            if (!validationResult.IsValid)
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });


            if (await dbContext.Coupons.AnyAsync(c => c.Code == cuoCreateDto.Code))
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new List<string> { "Coupon code already exists" }
                });

            var coupon = mapper.Map<Coupon>(cuoCreateDto);
            await dbContext.Coupons.AddAsync(coupon);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, new ApiResponse<CouponVm>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created,
                Data = mapper.Map<CouponVm>(coupon)
            });
        };
    }

    private static Func<CouponStoreDbContext, IMapper, int, Task<IResult>> GetCouponById()
    {
        return async (dbContext, mapper,
            [FromRoute] id) =>
        {
            var coupon = await dbContext.Coupons.FindAsync(id);
            if (coupon is null)
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                });


            return Results.Ok(new ApiResponse<CouponVm>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Data = mapper.Map<CouponVm>(coupon)
            });
        };
    }

    private static Func<CouponStoreDbContext, IMapper, Task<IResult>> GetAllCoupons()
    {
        return async (dbContext, mapper) =>
        {
            var coupons = await dbContext.Coupons.ToListAsync();
            var apiResponse = new ApiResponse<IReadOnlyCollection<CouponVm>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Data = coupons.Select(mapper.Map<CouponVm>).ToList()
            };
            return Results.Ok(apiResponse);
        };
    }
}