using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

/// <summary>
/// Interface do repositório de categorias
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Obtém todas as categorias ativas
    /// </summary>
    /// <returns>Coleção de categorias ativas</returns>
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
    
    /// <summary>
    /// Verifica se uma categoria pode ser excluída
    /// </summary>
    /// <param name="categoryId">ID da categoria</param>
    /// <returns>True se pode ser excluída</returns>
    Task<bool> CanDeleteAsync(int categoryId);
}