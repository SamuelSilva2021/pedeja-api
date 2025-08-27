# Repository Dinâmico - Exemplos de Uso

Este documento demonstra como usar a nova classe `Repository<T>` refatorada com predicates e consultas complexas.

## Funcionalidades Implementadas

### 1. Consultas com Predicates

```csharp
// Buscar produtos ativos
var activeProducts = await _productRepository.FindAsync(p => p.IsActive);

// Buscar primeiro produto por nome
var product = await _productRepository.FirstOrDefaultAsync(p => p.Name == "Pizza Margherita");

// Verificar se existe produto com preço específico
var exists = await _productRepository.AnyAsync(p => p.Price > 50.00m);

// Contar produtos por categoria
var count = await _productRepository.CountAsync(p => p.CategoryId == 1);
```

### 2. Paginação

```csharp
// Paginação simples
var page1 = await _productRepository.GetPagedAsync(1, 10); // Primeira página, 10 itens

// Paginação com filtro
var activePage = await _productRepository.GetPagedAsync(
    p => p.IsActive, 
    pageNumber: 2, 
    pageSize: 20
);
```

### 3. Ordenação

```csharp
// Ordenar todos os produtos por nome (ascendente)
var orderedProducts = await _productRepository.GetAllOrderedAsync(p => p.Name);

// Ordenar produtos ativos por preço (descendente)
var expensiveProducts = await _productRepository.FindOrderedAsync(
    p => p.IsActive,
    p => p.Price,
    ascending: false
);
```

### 4. Includes para Relacionamentos

```csharp
// Buscar todos os produtos com suas categorias
var productsWithCategories = await _productRepository.GetAllWithIncludesAsync(
    p => p.Category
);

// Buscar produto por ID com categoria e imagens
var productWithDetails = await _productRepository.GetByIdWithIncludesAsync(
    productId,
    p => p.Category,
    p => p.Images
);

// Buscar produtos ativos com múltiplos relacionamentos
var activeProductsWithDetails = await _productRepository.FindWithIncludesAsync(
    p => p.IsActive,
    p => p.Category,
    p => p.AddonGroups,
    p => p.Images
);
```

### 5. Operações em Lote

```csharp
// Adicionar múltiplos produtos
var newProducts = new List<Product> { /* produtos */ };
await _productRepository.AddRangeAsync(newProducts);

// Atualizar múltiplos produtos
var productsToUpdate = await _productRepository.FindAsync(p => p.CategoryId == 1);
foreach (var product in productsToUpdate)
{
    product.IsActive = false;
}
await _productRepository.UpdateRangeAsync(productsToUpdate);

// Deletar múltiplos produtos
var productsToDelete = await _productRepository.FindAsync(p => !p.IsActive);
await _productRepository.DeleteRangeAsync(productsToDelete);
```

### 6. Métodos de Agregação

```csharp
// Maior preço
var maxPrice = await _productRepository.MaxAsync(p => p.Price);

// Menor preço
var minPrice = await _productRepository.MinAsync(p => p.Price);

// Soma total dos preços
var totalValue = await _productRepository.SumAsync(p => p.Price);

// Preço médio
var averagePrice = await _productRepository.AverageAsync(p => p.Price);
```

## Exemplos Práticos de Uso em Services

### ProductService com Repository Dinâmico

```csharp
public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    
    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }
    
    // Buscar produtos por categoria com paginação
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(
        int categoryId, 
        int page, 
        int pageSize)
    {
        return await _productRepository.GetPagedAsync(
            p => p.CategoryId == categoryId && p.IsActive,
            page,
            pageSize
        );
    }
    
    // Buscar produtos em promoção
    public async Task<IEnumerable<Product>> GetPromotionalProductsAsync()
    {
        return await _productRepository.FindOrderedAsync(
            p => p.IsActive && p.PromotionalPrice.HasValue,
            p => p.PromotionalPrice.Value,
            ascending: true
        );
    }
    
    // Relatório de produtos por faixa de preço
    public async Task<ProductPriceReport> GetPriceReportAsync()
    {
        var activeProducts = p => p.IsActive;
        
        return new ProductPriceReport
        {
            TotalProducts = await _productRepository.CountAsync(activeProducts),
            MaxPrice = await _productRepository.MaxAsync(p => p.Price),
            MinPrice = await _productRepository.MinAsync(p => p.Price),
            AveragePrice = await _productRepository.AverageAsync(p => p.Price),
            TotalValue = await _productRepository.SumAsync(p => p.Price)
        };
    }
}
```

### AddonGroupService com Repository Dinâmico

```csharp
public class AddonGroupService
{
    private readonly IRepository<AddonGroup> _addonGroupRepository;
    
    public AddonGroupService(IRepository<AddonGroup> addonGroupRepository)
    {
        _addonGroupRepository = addonGroupRepository;
    }
    
    // Buscar grupos de adicionais por tipo
    public async Task<IEnumerable<AddonGroup>> GetAddonGroupsByTypeAsync(AddonGroupType type)
    {
        return await _addonGroupRepository.FindWithIncludesAsync(
            ag => ag.Type == type && ag.IsActive,
            ag => ag.Addons
        );
    }
    
    // Verificar se grupo tem adicionais obrigatórios
    public async Task<bool> HasRequiredAddonsAsync(int addonGroupId)
    {
        return await _addonGroupRepository.AnyAsync(
            ag => ag.Id == addonGroupId && ag.IsRequired
        );
    }
}
```

## Vantagens da Nova Implementação

1. **Flexibilidade**: Predicates permitem consultas dinâmicas sem criar métodos específicos
2. **Performance**: Includes otimizam carregamento de relacionamentos
3. **Paginação**: Suporte nativo para paginação eficiente
4. **Ordenação**: Ordenação dinâmica por qualquer propriedade
5. **Operações em Lote**: Melhor performance para operações múltiplas
6. **Agregações**: Métodos para cálculos estatísticos
7. **Reutilização**: Uma única classe serve para todas as entidades
8. **Testabilidade**: Interface bem definida facilita testes unitários

## Padrões Recomendados

1. **Use predicates** ao invés de criar métodos específicos no repositório
2. **Combine filtros** com paginação para melhor performance
3. **Use includes** apenas quando necessário para evitar over-fetching
4. **Prefira operações em lote** para múltiplas entidades
5. **Implemente validações** nos services antes de chamar o repositório
6. **Use agregações** para relatórios e estatísticas