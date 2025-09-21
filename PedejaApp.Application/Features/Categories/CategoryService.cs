using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Application.Features.Categories;
using Microsoft.Extensions.Logging;
using PedejaApp.Application.Common.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetCategoriesOrderedAsync();
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync();
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
    {
        // Validar nome único
        if (await _categoryRepository.IsNameUniqueAsync(createDto.Name))
        {
            throw new InvalidOperationException($"Já existe uma categoria com o nome '{createDto.Name}'");
        }

        var category = new Category
        {
            Name = createDto.Name,
            Description = createDto.Description,
            DisplayOrder = createDto.DisplayOrder ?? await _categoryRepository.GetNextDisplayOrderAsync(),
            IsActive = true
        };

        var createdCategory = await _categoryRepository.AddAsync(category);
        
        _logger.LogInformation("Categoria criada: {CategoryName} (ID: {CategoryId})", 
            createdCategory.Name, createdCategory.Id);

        return MapToDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
    {
        _logger.LogInformation("UpdateCategoryAsync called - ID: {Id}, UpdateDto: {@UpdateDto}", id, updateDto);
        
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException($"Categoria com ID {id} não encontrada");
        }

        // Apply partial updates
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
        {
            if (await _categoryRepository.IsNameUniqueAsync(updateDto.Name, id))
            {
                throw new InvalidOperationException($"Já existe uma categoria com o nome '{updateDto.Name}'");
            }
            category.Name = updateDto.Name;
        }

        // To clear the description, the client must send an empty string "".
        // Sending null or omitting the field will not change the existing value.
        if (updateDto.Description != null)
        {
            category.Description = updateDto.Description;
        }

        if (updateDto.DisplayOrder.HasValue)
        {
            category.DisplayOrder = updateDto.DisplayOrder.Value;
        }

        if (updateDto.IsActive.HasValue)
        {
            category.IsActive = updateDto.IsActive.Value;
        }

        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);

        _logger.LogInformation("Categoria atualizada: {CategoryName} (ID: {CategoryId})", 
            category.Name, category.Id);

        return MapToDto(category);
    }

    public async Task<ResultDto<bool>> DeleteCategoryAsync(int id)
    {
        try
        {
            if (!await CanDeleteCategoryAsync(id))
            {
                throw new InvalidOperationException("Não é possível excluir categoria que possui produtos associados");
            }

            var categoryEntity = await _categoryRepository.GetByIdAsync(id);
            await _categoryRepository.DeleteAsync(categoryEntity);

            return ResultDto<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ResultDto<bool>.Fail($"Erro ao excluir categoria: {ex.Message}");
        }
    }

    public async Task<bool> ReorderCategoriesAsync(ReorderCategoriesDto reorderDto)
    {
        await _categoryRepository.UpdateDisplayOrdersAsync(reorderDto.CategoryDisplayOrders);
        
        _logger.LogInformation("Ordem das categorias atualizada: {Count} categorias reordenadas", 
            reorderDto.CategoryDisplayOrders.Count);
        
        return true;
    }

    public async Task<bool> CanDeleteCategoryAsync(int id)
    {
        return !await _categoryRepository.HasProductsAsync(id);
    }

    public async Task<CategoryDto> ToggleCategoryActiveAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException($"Categoria com ID {id} não encontrada");
        }

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);

        _logger.LogInformation("Status da categoria alterado: {CategoryName} (ID: {CategoryId}) - Ativa: {IsActive}", 
            category.Name, category.Id, category.IsActive);

        return MapToDto(category);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}