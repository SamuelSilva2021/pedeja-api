using PedejaApp.Application.Common.Models;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

/// <summary>
/// Implementação do serviço de validação de grupos de adicionais de produtos usando recursos modernos do C# 13
/// </summary>
public class ProductAddonGroupValidationService(
    IProductRepository productRepository,
    IAddonGroupRepository addonGroupRepository,
    IProductAddonGroupRepository productAddonGroupRepository,
    IOrderRepository orderRepository) : IProductAddonGroupValidationService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IAddonGroupRepository _addonGroupRepository = addonGroupRepository;
    private readonly IProductAddonGroupRepository _productAddonGroupRepository = productAddonGroupRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;

    /// <summary>
    /// Valida se um produto existe e está ativo
    /// </summary>
    public async Task<bool> IsProductValidAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product != null && product.IsActive;
    }

    /// <summary>
    /// Valida se um grupo de adicionais existe e está ativo
    /// </summary>
    public async Task<bool> IsAddonGroupValidAsync(int addonGroupId)
    {
        var addonGroup = await _addonGroupRepository.GetByIdAsync(addonGroupId);
        return addonGroup != null && addonGroup.IsActive;
    }

    /// <summary>
    /// Valida se um grupo de adicionais já está associado a um produto
    /// </summary>
    public async Task<bool> IsAddonGroupAlreadyAssignedAsync(int productId, int addonGroupId)
    {
        var productAddonGroups = await _productAddonGroupRepository.GetByProductIdAsync(productId);
        return productAddonGroups.Any(pag => pag.AddonGroupId == addonGroupId);
    }

    /// <summary>
    /// Valida se um ProductAddonGroup pode ser excluído (não está em pedidos ativos)
    /// </summary>
    public async Task<bool> CanDeleteProductAddonGroupAsync(int productAddonGroupId)
    {
        // Verifica se o ProductAddonGroup existe em pedidos ativos
        var activeOrders = await _orderRepository.GetActiveOrdersWithProductAddonGroupAsync(productAddonGroupId);
        return !activeOrders.Any();
    }

    /// <summary>
    /// Valida dados de adição de grupo de adicionais a produto
    /// </summary>
    public async Task<ValidationResult> ValidateAddAddonGroupToProductAsync(int productId, AddProductAddonGroupRequestDto request)
    {
        var errors = new List<string>();

        // Validação de produto
        if (!await IsProductValidAsync(productId))
            errors.Add("Produto não encontrado ou inativo");

        // Validação de grupo de adicionais
        if (!await IsAddonGroupValidAsync(request.AddonGroupId))
            errors.Add("Grupo de adicionais não encontrado ou inativo");
        

        // Validação de duplicação
        if (await IsAddonGroupAlreadyAssignedAsync(productId, request.AddonGroupId))
            errors.Add("Este grupo de adicionais já está associado ao produto");
        

        // Validações de negócio
        if (!AreSelectionsValid(request.MinSelectionsOverride, request.MaxSelectionsOverride))
            errors.Add("As seleções mínimas não podem ser maiores que as máximas");

        if (!IsDisplayOrderValid(request.DisplayOrder))
            errors.Add("A ordem de exibição deve ser um valor positivo");

        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure("Erro na validação dos dados", errors);
    }

    /// <summary>
    /// Valida dados de atualização de grupo de adicionais de produto
    /// </summary>
    public async Task<ValidationResult> ValidateUpdateProductAddonGroupAsync(int productAddonGroupId, UpdateProductAddonGroupRequestDto request)
    {
        var errors = new List<string>();

        // Verifica se o ProductAddonGroup existe
        var existingProductAddonGroup = await _productAddonGroupRepository.GetByIdAsync(productAddonGroupId);
        if (existingProductAddonGroup == null)
        {
            errors.Add("Associação de grupo de adicionais não encontrada");
            return ValidationResult.Failure("Associação não encontrada", errors);
        }

        // Validações de negócio
        if (!AreSelectionsValid(request.MinSelectionsOverride, request.MaxSelectionsOverride))
            errors.Add("As seleções mínimas não podem ser maiores que as máximas");

        if (!IsDisplayOrderValid(request.DisplayOrder))
            errors.Add("A ordem de exibição deve ser um valor positivo");

        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure("Erro na validação dos dados", errors);
    }

    /// <summary>
    /// Valida dados de adição em lote de grupos de adicionais
    /// </summary>
    public async Task<ValidationResult> ValidateBulkAddAddonGroupsAsync(int productId, IEnumerable<AddProductAddonGroupRequestDto> requests)
    {
        var errors = new List<string>();
        var requestList = requests.ToList();

        // Validação de produto uma vez
        if (!await IsProductValidAsync(productId))
        {
            errors.Add("Produto não encontrado ou inativo");
            return ValidationResult.Failure("Produto inválido", errors);
        }

        // Validação de duplicação de grupos na requisição
        var duplicateGroups = requestList
            .GroupBy(r => r.AddonGroupId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateGroups.Any())
            errors.Add($"Grupos de adicionais duplicados na requisição: {string.Join(", ", duplicateGroups)}");

        // Validação individual de cada request
        for (int i = 0; i < requestList.Count; i++)
        {
            var request = requestList[i];
            var itemErrors = new List<string>();

            // Validação de grupo de adicionais
            if (!await IsAddonGroupValidAsync(request.AddonGroupId))
                itemErrors.Add($"Item {i + 1}: Grupo de adicionais {request.AddonGroupId} não encontrado ou inativo");

            // Validação de duplicação com grupos já existentes
            if (await IsAddonGroupAlreadyAssignedAsync(productId, request.AddonGroupId))
                itemErrors.Add($"Item {i + 1}: Grupo de adicionais {request.AddonGroupId} já está associado ao produto");

            // Validações de negócio
            if (!AreSelectionsValid(request.MinSelectionsOverride, request.MaxSelectionsOverride))
                itemErrors.Add($"Item {i + 1}: Seleções mínimas não podem ser maiores que as máximas");

            if (!IsDisplayOrderValid(request.DisplayOrder))
                itemErrors.Add($"Item {i + 1}: Ordem de exibição deve ser um valor positivo");

            errors.AddRange(itemErrors);
        }

        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure("Erro na validação dos dados em lote", errors);
    }

    /// <summary>
    /// Valida se as seleções mínimas e máximas são consistentes
    /// </summary>
    public bool AreSelectionsValid(int? minSelections, int? maxSelections)
    {
        // Se ambos são nulos, é válido
        if (!minSelections.HasValue && !maxSelections.HasValue)
            return true;

        // Se apenas um é nulo, é válido
        if (!minSelections.HasValue || !maxSelections.HasValue)
            return true;

        // Se ambos têm valor, min deve ser <= max
        return minSelections.Value <= maxSelections.Value && minSelections.Value >= 0 && maxSelections.Value >= 0;
    }

    /// <summary>
    /// Valida se a ordem de exibição é válida
    /// </summary>
    public bool IsDisplayOrderValid(int displayOrder) => displayOrder >= 0;
}