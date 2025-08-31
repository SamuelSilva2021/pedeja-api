using AutoMapper;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Mappings;

/// <summary>
/// Profile de mapeamento do AutoMapper para entidades Product
/// Utiliza recursos modernos do C# 13 e .NET 9
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Mapeamento de Product para ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // Será tratado pelo UrlBuilderService

        // Mapeamento de CreateProductRequestDto para Product
        CreateMap<CreateProductRequestDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.AddonGroups, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Mapeamento de UpdateProductRequest para Product (para atualização)
        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.AddonGroups, opt => opt.Ignore()) // Ignorar propriedade de navegação
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Mapeamento reverso para casos específicos
        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore());
    }
}