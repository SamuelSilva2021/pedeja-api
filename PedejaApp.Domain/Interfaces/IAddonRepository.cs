using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

/// <summary>
/// Interface para repositório de adicionais
/// </summary>
public interface IAddonRepository
{
    /// <summary>
    /// Obtém todos os adicionais ativos
    /// </summary>
    Task<IEnumerable<Addon>> GetActiveAddonsAsync();
    
    /// <summary>
    /// Obtém adicional por ID
    /// </summary>
    Task<Addon?> GetAddonByIdAsync(int id);
    
    /// <summary>
    /// Obtém adicionais por grupo
    /// </summary>
    Task<IEnumerable<Addon>> GetAddonsByGroupIdAsync(int groupId);
    
    /// <summary>
    /// Cria um novo adicional
    /// </summary>
    Task<Addon> CreateAddonAsync(Addon addon);
    
    /// <summary>
    /// Atualiza um adicional existente
    /// </summary>
    Task<Addon> UpdateAddonAsync(Addon addon);
    
    /// <summary>
    /// Exclui um adicional
    /// </summary>
    Task DeleteAddonAsync(int id);
    
    /// <summary>
    /// Verifica se um adicional pode ser excluído
    /// </summary>
    Task<bool> CanDeleteAddonAsync(int id);
}