using PedejaApp.Application.Features.Categories;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
    Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto);
    Task<ResultDto<bool>> DeleteCategoryAsync(int id);
    Task<bool> ReorderCategoriesAsync(ReorderCategoriesDto reorderDto);
    Task<bool> CanDeleteCategoryAsync(int id);
    Task<CategoryDto> ToggleCategoryActiveAsync(int id);
}