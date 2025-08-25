using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

/// <summary>
/// Interface para repositório de grupos de adicionais
/// </summary>
public interface IAddonGroupRepository
{
    /// <summary>
    /// Obtém todos os grupos de adicionais ativos
    /// </summary>
    Task<IEnumerable<AddonGroup>> GetActiveAddonGroupsAsync();
    
    /// <summary>
    /// Obtém grupo de adicionais por ID
    /// </summary>
    Task<AddonGroup?> GetAddonGroupByIdAsync(int id);
    
    /// <summary>
    /// Obtém grupos de adicionais por produto
    /// </summary>
    Task<IEnumerable<AddonGroup>> GetAddonGroupsByProductIdAsync(int productId);
    
    /// <summary>
    /// Cria um novo grupo de adicionais
    /// </summary>
    Task<AddonGroup> CreateAddonGroupAsync(AddonGroup addonGroup);
    
    /// <summary>
    /// Atualiza um grupo de adicionais existente
    /// </summary>
    Task<AddonGroup> UpdateAddonGroupAsync(AddonGroup addonGroup);
    
    /// <summary>
    /// Exclui um grupo de adicionais
    /// </summary>
    Task DeleteAddonGroupAsync(int id);
    
    /// <summary>
    /// Verifica se um grupo de adicionais pode ser excluído
    /// </summary>
    Task<bool> CanDeleteAddonGroupAsync(int id);
    
    /// <summary>
    /// Verifica se um nome de grupo já existe
    /// </summary>
    Task<bool> IsGroupNameUniqueAsync(string name, int? excludeId = null);
}