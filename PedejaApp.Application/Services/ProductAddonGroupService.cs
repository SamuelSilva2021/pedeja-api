using Microsoft.EntityFrameworkCore;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Infrastructure.Data;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services
{
    public class ProductAddonGroupService : IProductAddonGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductAddonGroupService> _logger;

        public ProductAddonGroupService(ApplicationDbContext context, ILogger<ProductAddonGroupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductAddonGroup>> GetProductAddonGroupsAsync(int productId)
        {
            return await _context.ProductAddonGroups
                .Include(pag => pag.AddonGroup)
                    .ThenInclude(ag => ag.Addons.Where(a => a.IsActive))
                .Where(pag => pag.ProductId == productId)
                .OrderBy(pag => pag.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ProductWithAddonsResponseDto?> GetProductWithAddonsAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.AddonGroups)
                    .ThenInclude(pag => pag.AddonGroup)
                        .ThenInclude(ag => ag.Addons.Where(a => a.IsActive))
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return null;

            return new ProductWithAddonsResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                AddonGroups = product.AddonGroups
                    .OrderBy(pag => pag.DisplayOrder)
                    .Select(pag => new AddonGroupResponseDto
                    {
                        Id = pag.AddonGroup.Id,
                        Name = pag.AddonGroup.Name,
                        Description = pag.AddonGroup.Description,
                        Type = pag.AddonGroup.Type,
                        MinSelections = pag.MinSelectionsOverride ?? pag.AddonGroup.MinSelections,
                        MaxSelections = pag.MaxSelectionsOverride ?? pag.AddonGroup.MaxSelections,
                        IsRequired = pag.IsRequired,
                        DisplayOrder = pag.DisplayOrder,
                        Addons = pag.AddonGroup.Addons
                            .OrderBy(a => a.DisplayOrder)
                            .Select(a => new AddonResponseDto
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Description = a.Description,
                                Price = a.Price,
                                ImageUrl = a.ImageUrl,
                                DisplayOrder = a.DisplayOrder,
                                AddonGroupId = a.AddonGroupId,
                                IsActive = a.IsActive
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<ProductAddonGroup> AddAddonGroupToProductAsync(int productId, AddProductAddonGroupRequest request)
        {
            // Verificar se produto existe
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                throw new ArgumentException($"Produto com ID {productId} não encontrado");

            // Verificar se grupo de adicionais existe
            var addonGroupExists = await _context.AddonGroups.AnyAsync(ag => ag.Id == request.AddonGroupId);
            if (!addonGroupExists)
                throw new ArgumentException($"Grupo de adicionais com ID {request.AddonGroupId} não encontrado");

            // Verificar se já não está associado
            var existingAssociation = await _context.ProductAddonGroups
                .FirstOrDefaultAsync(pag => pag.ProductId == productId && pag.AddonGroupId == request.AddonGroupId);
            
            if (existingAssociation != null)
                throw new InvalidOperationException("Este grupo de adicionais já está associado ao produto");

            var productAddonGroup = new ProductAddonGroup
            {
                ProductId = productId,
                AddonGroupId = request.AddonGroupId,
                DisplayOrder = request.DisplayOrder,
                IsRequired = request.IsRequired,
                MinSelectionsOverride = request.MinSelectionsOverride,
                MaxSelectionsOverride = request.MaxSelectionsOverride
            };

            _context.ProductAddonGroups.Add(productAddonGroup);
            await _context.SaveChangesAsync();

            // Recarregar com navegação
            return await _context.ProductAddonGroups
                .Include(pag => pag.AddonGroup)
                .FirstAsync(pag => pag.Id == productAddonGroup.Id);
        }

        public async Task<ProductAddonGroup> UpdateProductAddonGroupAsync(int productId, int addonGroupId, UpdateProductAddonGroupRequest request)
        {
            var productAddonGroup = await _context.ProductAddonGroups
                .Include(pag => pag.AddonGroup)
                .FirstOrDefaultAsync(pag => pag.ProductId == productId && pag.AddonGroupId == addonGroupId);

            if (productAddonGroup == null)
                throw new ArgumentException("Associação produto-grupo de adicionais não encontrada");

            productAddonGroup.DisplayOrder = request.DisplayOrder;
            productAddonGroup.IsRequired = request.IsRequired;
            productAddonGroup.MinSelectionsOverride = request.MinSelectionsOverride;
            productAddonGroup.MaxSelectionsOverride = request.MaxSelectionsOverride;

            await _context.SaveChangesAsync();
            return productAddonGroup;
        }

        public async Task RemoveAddonGroupFromProductAsync(int productId, int addonGroupId)
        {
            var productAddonGroup = await _context.ProductAddonGroups
                .FirstOrDefaultAsync(pag => pag.ProductId == productId && pag.AddonGroupId == addonGroupId);

            if (productAddonGroup == null)
                throw new ArgumentException("Associação produto-grupo de adicionais não encontrada");

            _context.ProductAddonGroups.Remove(productAddonGroup);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderProductAddonGroupsAsync(int productId, Dictionary<int, int> groupOrders)
        {
            var productAddonGroups = await _context.ProductAddonGroups
                .Where(pag => pag.ProductId == productId)
                .ToListAsync();

            foreach (var pag in productAddonGroups)
            {
                if (groupOrders.TryGetValue(pag.AddonGroupId, out int newOrder))
                {
                    pag.DisplayOrder = newOrder;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductAddonGroup>> BulkAddAddonGroupsToProductAsync(int productId, IEnumerable<AddProductAddonGroupRequest> requests)
        {
            // Verificar se produto existe
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                throw new ArgumentException($"Produto com ID {productId} não encontrado");

            var addonGroupIds = requests.Select(r => r.AddonGroupId).ToList();
            
            // Verificar se todos os grupos existem
            var existingGroups = await _context.AddonGroups
                .Where(ag => addonGroupIds.Contains(ag.Id))
                .Select(ag => ag.Id)
                .ToListAsync();

            var missingGroups = addonGroupIds.Except(existingGroups).ToList();
            if (missingGroups.Any())
                throw new ArgumentException($"Grupos de adicionais não encontrados: {string.Join(", ", missingGroups)}");

            // Verificar se já não estão associados
            var existingAssociations = await _context.ProductAddonGroups
                .Where(pag => pag.ProductId == productId && addonGroupIds.Contains(pag.AddonGroupId))
                .Select(pag => pag.AddonGroupId)
                .ToListAsync();

            var newAssociations = requests.Where(r => !existingAssociations.Contains(r.AddonGroupId));

            var productAddonGroups = newAssociations.Select(request => new ProductAddonGroup
            {
                ProductId = productId,
                AddonGroupId = request.AddonGroupId,
                DisplayOrder = request.DisplayOrder,
                IsRequired = request.IsRequired,
                MinSelectionsOverride = request.MinSelectionsOverride,
                MaxSelectionsOverride = request.MaxSelectionsOverride
            }).ToList();

            _context.ProductAddonGroups.AddRange(productAddonGroups);
            await _context.SaveChangesAsync();

            // Recarregar com navegação
            var addedIds = productAddonGroups.Select(pag => pag.Id).ToList();
            return await _context.ProductAddonGroups
                .Include(pag => pag.AddonGroup)
                .Where(pag => addedIds.Contains(pag.Id))
                .ToListAsync();
        }

        public async Task BulkRemoveAddonGroupsFromProductAsync(int productId, IEnumerable<int> addonGroupIds)
        {
            var productAddonGroups = await _context.ProductAddonGroups
                .Where(pag => pag.ProductId == productId && addonGroupIds.Contains(pag.AddonGroupId))
                .ToListAsync();

            _context.ProductAddonGroups.RemoveRange(productAddonGroups);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAddonGroupAssignedToProductAsync(int productId, int addonGroupId)
        {
            return await _context.ProductAddonGroups
                .AnyAsync(pag => pag.ProductId == productId && pag.AddonGroupId == addonGroupId);
        }

        public async Task<bool> CanRemoveAddonGroupFromProductAsync(int productId, int addonGroupId)
        {
            // Verificar se há pedidos pendentes que usam este produto com adicionais
            // Por enquanto, retorna true - pode ser implementado futuramente
            return true;
        }

        public async Task<IEnumerable<Product>> GetProductsWithAddonGroupAsync(int addonGroupId)
        {
            return await _context.ProductAddonGroups
                .Include(pag => pag.Product)
                .Where(pag => pag.AddonGroupId == addonGroupId)
                .Select(pag => pag.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductWithAddonsResponseDto>> GetAllProductsWithAddonsAsync()
        {
            var products = await _context.Products
                .Include(p => p.AddonGroups)
                    .ThenInclude(pag => pag.AddonGroup)
                        .ThenInclude(ag => ag.Addons.Where(a => a.IsActive))
                .Where(p => p.IsActive)
                .ToListAsync();

            return products.Select(product => new ProductWithAddonsResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                AddonGroups = product.AddonGroups
                    .OrderBy(pag => pag.DisplayOrder)
                    .Select(pag => new AddonGroupResponseDto
                    {
                        Id = pag.AddonGroup.Id,
                        Name = pag.AddonGroup.Name,
                        Description = pag.AddonGroup.Description,
                        Type = pag.AddonGroup.Type,
                        MinSelections = pag.MinSelectionsOverride ?? pag.AddonGroup.MinSelections,
                        MaxSelections = pag.MaxSelectionsOverride ?? pag.AddonGroup.MaxSelections,
                        IsRequired = pag.IsRequired,
                        DisplayOrder = pag.DisplayOrder,
                        Addons = pag.AddonGroup.Addons
                            .OrderBy(a => a.DisplayOrder)
                            .Select(a => new AddonResponseDto
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Description = a.Description,
                                Price = a.Price,
                                ImageUrl = a.ImageUrl,
                                DisplayOrder = a.DisplayOrder,
                                AddonGroupId = a.AddonGroupId,
                                IsActive = a.IsActive
                            }).ToList()
                    }).ToList()
            }).ToList();
        }
    }
}
