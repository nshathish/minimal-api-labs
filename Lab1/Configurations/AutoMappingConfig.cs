using AutoMapper;
using Lab1.Data.Entities;
using Lab1.Models.Dtos;
using Lab1.Models.ViewModels;

namespace Lab1.Configurations;

public class AutoMappingConfig : Profile
{
    public AutoMappingConfig()
    {
        // Coupon -> CouponVm
        CreateMap<Coupon, CouponVm>();

        // CouponCreateDto -> Coupon
        CreateMap<CouponCreateDto, Coupon>()
            .ForMember(des => des.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}