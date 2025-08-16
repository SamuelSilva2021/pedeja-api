using PedejaApp.Domain.Entities;

namespace PedejaApp.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(int id, Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> CanDeleteCategoryAsync(int id);
    Task<bool> UpdateDisplayOrdersAsync(Dictionary<int, int> categoryDisplayOrders);
}
