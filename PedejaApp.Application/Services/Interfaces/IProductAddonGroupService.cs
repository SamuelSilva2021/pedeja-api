using PedejaApp.Domain.Entities;
using PedejaApp.Domain.DTOs;

namespace PedejaApp.Application.Services.Interfaces
{
    public interface IProductAddonGroupService
    {
        // Obter grupos de adicionais de um produto
        Task<IEnumerable<ProductAddonGroup>> GetProductAddonGroupsAsync(int productId);
        
        // Obter produto com todos os grupos de adicionais
        Task<ProductWithAddonsResponseDto?> GetProductWithAddonsAsync(int productId);
        
        // Adicionar grupo de adicionais a um produto
        Task<ProductAddonGroup> AddAddonGroupToProductAsync(int productId, AddProductAddonGroupRequestDto request);
        
        // Atualizar configuração de grupo de adicionais em um produto
        Task<ProductAddonGroup> UpdateProductAddonGroupAsync(int productId, int addonGroupId, UpdateProductAddonGroupRequestDto request);
        
        // Remover grupo de adicionais de um produto
        Task RemoveAddonGroupFromProductAsync(int productId, int addonGroupId);
        
        // Reordenar grupos de adicionais de um produto
        Task ReorderProductAddonGroupsAsync(int productId, Dictionary<int, int> groupOrders);
        
        // Operações em lote
        Task<IEnumerable<ProductAddonGroup>> BulkAddAddonGroupsToProductAsync(int productId, IEnumerable<AddProductAddonGroupRequestDto> requests);
        Task BulkRemoveAddonGroupsFromProductAsync(int productId, IEnumerable<int> addonGroupIds);
        
        // Validações
        Task<bool> IsAddonGroupAssignedToProductAsync(int productId, int addonGroupId);
        Task<bool> CanRemoveAddonGroupFromProductAsync(int productId, int addonGroupId);
        
        // Busca e filtros
        Task<IEnumerable<Product>> GetProductsWithAddonGroupAsync(int addonGroupId);
        Task<IEnumerable<ProductWithAddonsResponseDto>> GetAllProductsWithAddonsAsync();
    }
}
