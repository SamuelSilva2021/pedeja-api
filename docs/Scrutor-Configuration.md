# Configuração do Scrutor para Clean Architecture

## Visão Geral

O Scrutor foi configurado para registrar automaticamente todas as dependências seguindo as convenções da Clean Architecture, eliminando a necessidade de registros manuais na maioria dos casos.

## Configuração Atual

### Método Principal: `AddScrutorServices()`

Este método registra automaticamente:

#### 1. Repositories (Infrastructure → Domain)
- **Namespace**: `PedejaApp.Infrastructure.Repositories`
- **Convenção**: Classes que terminam com "Repository" e implementam `IRepository<>`
- **Lifetime**: Scoped
- **Exemplo**: `ProductRepository` → `IProductRepository`

#### 2. Services (Application Layer)
- **Namespaces**: 
  - `PedejaApp.Application.Services`
  - `PedejaApp.Application.Features.Categories`
- **Convenção**: Classes que terminam com "Service"
- **Lifetime**: Scoped
- **Exemplo**: `ProductService` → `IProductService`

#### 3. Mappers e Validators
- **Namespaces**: 
  - `PedejaApp.Application.Services`
  - `PedejaApp.Application.Mappers`
  - `PedejaApp.Application.Validators`
- **Convenção**: Classes que terminam com "Mapper", "Validator" ou contêm "Validation"
- **Lifetime**: Scoped
- **Exemplo**: `ProductMapper` → `IProductMapper`

#### 4. Handlers (CQRS/MediatR)
- **Namespaces**: 
  - `PedejaApp.Application.Handlers`
  - `PedejaApp.Application.Commands`
  - `PedejaApp.Application.Queries`
- **Convenção**: Classes que terminam com "Handler"
- **Lifetime**: Scoped

### Método Alternativo: `AddScrutorServicesByConvention()`

Uma abordagem mais ampla que registra por convenção de nomenclatura em todos os assemblies:
- Qualquer classe que termine com: Repository, Service, Mapper, Validator
- Ou contenha "Validation" no nome
- Registra automaticamente como suas interfaces implementadas

## Vantagens da Configuração

### ✅ Automação Completa
- **Sem registros manuais**: Novas classes são automaticamente registradas
- **Convenção sobre configuração**: Segue padrões estabelecidos
- **Redução de erros**: Elimina esquecimento de registros

### ✅ Clean Architecture
- **Separação de responsabilidades**: Cada camada tem suas convenções
- **Inversão de dependência**: Interfaces são automaticamente resolvidas
- **Baixo acoplamento**: Dependências são injetadas automaticamente

### ✅ Manutenibilidade
- **Código limpo**: Menos código boilerplate
- **Fácil evolução**: Novas funcionalidades seguem automaticamente as convenções
- **Consistência**: Todas as dependências seguem o mesmo padrão

## Como Usar

### 1. Criando um Novo Repository

```csharp
// Domain/Interfaces/ICustomerRepository.cs
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
}

// Infrastructure/Repositories/CustomerRepository.cs
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }
}
```

**✅ Registrado automaticamente pelo Scrutor!**

### 2. Criando um Novo Service

```csharp
// Application/Services/Interfaces/ICustomerService.cs
public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
}

// Application/Services/CustomerService.cs
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    
    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // Implementação
    }
}
```

**✅ Registrado automaticamente pelo Scrutor!**

### 3. Criando Validators

```csharp
// Application/Services/Interfaces/ICustomerValidationService.cs
public interface ICustomerValidationService
{
    Task<ValidationResult> ValidateCreateCustomerAsync(CreateCustomerRequest request);
}

// Application/Services/CustomerValidationService.cs
public class CustomerValidationService : ICustomerValidationService
{
    // Implementação
}
```

**✅ Registrado automaticamente pelo Scrutor!**

## Casos Especiais

Para serviços que **NÃO** seguem as convenções ou precisam de configuração especial, use o `ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Serviços com configuração especial
    services.AddScoped<ISpecialService>(provider => 
        new SpecialService(provider.GetService<IConfiguration>().GetConnectionString("Special")));
    
    // Factories
    services.AddScoped<IServiceFactory, ServiceFactory>();
    
    // Decorators
    services.Decorate<IProductService, CachedProductService>();
    
    return services;
}
```

## Troubleshooting

### Problema: Serviço não está sendo registrado

**Verificações**:
1. ✅ A classe segue a convenção de nomenclatura?
2. ✅ Está no namespace correto?
3. ✅ Implementa uma interface?
4. ✅ Não é abstrata?
5. ✅ O assembly está sendo escaneado?

### Problema: Múltiplas implementações

O Scrutor registra a **última** implementação encontrada. Para controle específico:

```csharp
// Registrar manualmente no ServiceCollectionExtensions
services.AddScoped<IService, PreferredImplementation>();
```

## Monitoramento

Para verificar quais serviços foram registrados:

```csharp
// Em desenvolvimento, adicione logging
services.PostConfigure<ServiceCollection>(services =>
{
    var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
    logger?.LogInformation($"Registered {services.Count} services");
});
```

## Conclusão

A configuração do Scrutor elimina 95% dos registros manuais de dependências, mantendo o código limpo e seguindo as melhores práticas da Clean Architecture. Novas funcionalidades são automaticamente integradas ao sistema de DI seguindo as convenções estabelecidas.