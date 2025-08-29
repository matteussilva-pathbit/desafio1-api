using Application.DTOs.Product;   // ✅ troque para Application.DTOs
using AutoMapper;
using Domain.Entities;

namespace API.Profiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
        CreateMap<Product, ProductReadDto>();
    }
}
