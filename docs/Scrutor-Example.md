# Exemplo Prático: Configuração do Scrutor em Ação

## Cenário: Adicionando um Novo Módulo de Clientes

Vamos demonstrar como a configuração do Scrutor automatiza o registro de dependências ao adicionar um novo módulo completo.

## 1. Estrutura do Módulo Cliente

### Domain Layer - Interface do Repository

```csharp
// PedejaApp.Domain/Interfaces/ICustomerRepository.cs
using PedejaApp.Domain.Entities;

namespace PedejaApp.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
```

### Infrastructure Layer - Implementação do Repository

```csharp
// PedejaApp.Infrastructure/Repositories/CustomerRepository.cs
using Microsoft.EntityFrameworkCore;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Infrastructure.Data;

namespace PedejaApp.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _context.Customers.Where(c => c.Email.ToLower() == email.ToLower());
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
            
        return !await query.AnyAsync();
    }
}
```

### Application Layer - Interface do Service

```csharp
// PedejaApp.Application/Services/Interfaces/ICustomerService.cs
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int id);
}
```

### Application Layer - Implementação do Service

```csharp
// PedejaApp.Application/Services/CustomerService.cs
using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerMapper _customerMapper;
    private readonly ICustomerValidationService _validationService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICustomerMapper customerMapper,
        ICustomerValidationService validationService,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _customerMapper = customerMapper;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _customerMapper.MapToDtos(customers);
    }

    public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync()
    {
        var customers = await _customerRepository.GetActiveCustomersAsync();
        return _customerMapper.MapToDtos(customers);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer != null ? _customerMapper.MapToDto(customer) : null;
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        var customer = await _customerRepository.GetByEmailAsync(email);
        return customer != null ? _customerMapper.MapToDto(customer) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // Validação
        var validationResult = await _validationService.ValidateCreateCustomerAsync(request);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join(", ", validationResult.Errors));

        // Mapeamento e criação
        var customer = _customerMapper.MapToEntity(request);
        var createdCustomer = await _customerRepository.AddAsync(customer);
        
        _logger.LogInformation("Cliente criado: {CustomerId} - {CustomerName}", 
            createdCustomer.Id, createdCustomer.Name);
            
        return _customerMapper.MapToDto(createdCustomer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(id);
        if (existingCustomer == null)
            throw new ArgumentException("Cliente não encontrado");

        // Validação
        var validationResult = await _validationService.ValidateUpdateCustomerAsync(id, request);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join(", ", validationResult.Errors));

        // Atualização
        _customerMapper.MapToEntity(request, existingCustomer);
        var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
        
        _logger.LogInformation("Cliente atualizado: {CustomerId} - {CustomerName}", 
            updatedCustomer.Id, updatedCustomer.Name);
            
        return _customerMapper.MapToDto(updatedCustomer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return false;

        await _customerRepository.DeleteAsync(id);
        
        _logger.LogInformation("Cliente excluído: {CustomerId} - {CustomerName}", 
            customer.Id, customer.Name);
            
        return true;
    }
}
```

### Application Layer - Mapper

```csharp
// PedejaApp.Application/Services/Interfaces/ICustomerMapper.cs
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

public interface ICustomerMapper
{
    CustomerDto MapToDto(Customer customer);
    IEnumerable<CustomerDto> MapToDtos(IEnumerable<Customer> customers);
    Customer MapToEntity(CreateCustomerRequest request);
    void MapToEntity(UpdateCustomerRequest request, Customer customer);
}

// PedejaApp.Application/Services/CustomerMapper.cs
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

public class CustomerMapper : ICustomerMapper
{
    public CustomerDto MapToDto(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public IEnumerable<CustomerDto> MapToDtos(IEnumerable<Customer> customers)
    {
        ArgumentNullException.ThrowIfNull(customers);
        return customers.Select(MapToDto);
    }

    public Customer MapToEntity(CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var now = DateTime.UtcNow;
        
        return new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            Address = request.Address?.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MapToEntity(UpdateCustomerRequest request, Customer customer)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(customer);
        
        customer.Name = request.Name.Trim();
        customer.Email = request.Email.Trim().ToLower();
        customer.Phone = request.Phone.Trim();
        customer.Address = request.Address?.Trim();
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;
    }
}
```

### Application Layer - Validation Service

```csharp
// PedejaApp.Application/Services/Interfaces/ICustomerValidationService.cs
using PedejaApp.Application.Common.Models;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services.Interfaces;

public interface ICustomerValidationService
{
    Task<ValidationResult> ValidateCreateCustomerAsync(CreateCustomerRequest request);
    Task<ValidationResult> ValidateUpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}

// PedejaApp.Application/Services/CustomerValidationService.cs
using PedejaApp.Application.Common.Models;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Application.Services;

public class CustomerValidationService : ICustomerValidationService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerValidationService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ValidationResult> ValidateCreateCustomerAsync(CreateCustomerRequest request)
    {
        var errors = new List<string>();

        // Validar email único
        if (!await IsEmailUniqueAsync(request.Email))
            errors.Add("Email já está em uso por outro cliente");

        // Validar telefone único
        var existingCustomerByPhone = await _customerRepository.GetByPhoneAsync(request.Phone);
        if (existingCustomerByPhone != null)
            errors.Add("Telefone já está em uso por outro cliente");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    public async Task<ValidationResult> ValidateUpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var errors = new List<string>();

        // Validar se cliente existe
        var existingCustomer = await _customerRepository.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            errors.Add("Cliente não encontrado");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Validar email único (excluindo o próprio cliente)
        if (!await IsEmailUniqueAsync(request.Email, id))
            errors.Add("Email já está em uso por outro cliente");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        return await _customerRepository.IsEmailUniqueAsync(email, excludeId);
    }
}
```

## 2. Resultado: Registro Automático

### ✅ O que foi registrado automaticamente pelo Scrutor:

1. **CustomerRepository** → **ICustomerRepository** (Scoped)
2. **CustomerService** → **ICustomerService** (Scoped)
3. **CustomerMapper** → **ICustomerMapper** (Scoped)
4. **CustomerValidationService** → **ICustomerValidationService** (Scoped)

### ✅ Nenhum registro manual necessário!

Todos os serviços seguem as convenções:
- ✅ Terminam com "Repository", "Service", "Mapper", "ValidationService"
- ✅ Estão nos namespaces corretos
- ✅ Implementam interfaces
- ✅ Não são abstratos

## 3. Uso no Controller

```csharp
// PedejaApp.Web/Controllers/CustomersController.cs
using Microsoft.AspNetCore.Mvc;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Web.Models.DTOs;

namespace PedejaApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    // ✅ Injeção automática - Scrutor registrou tudo!
    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return customer != null ? Ok(customer) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, UpdateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.UpdateCustomerAsync(id, request);
            return Ok(customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        var deleted = await _customerService.DeleteCustomerAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
```

## 4. Vantagens Demonstradas

### 🚀 **Produtividade**
- **Zero configuração manual** de DI
- **Foco no código de negócio** ao invés de infraestrutura
- **Desenvolvimento mais rápido** de novas funcionalidades

### 🔧 **Manutenibilidade**
- **Convenções consistentes** em todo o projeto
- **Menos código boilerplate** para manter
- **Evolução natural** do sistema

### ✅ **Confiabilidade**
- **Impossível esquecer** de registrar dependências
- **Detecção automática** de novas classes
- **Padrão uniforme** de registro

### 🏗️ **Arquitetura**
- **Clean Architecture** respeitada automaticamente
- **Separação de responsabilidades** clara
- **Inversão de dependência** automática

## 5. Conclusão

Com a configuração do Scrutor, adicionar um módulo completo (Repository + Service + Mapper + Validator) requer **ZERO configuração manual** de injeção de dependência. O sistema automaticamente:

1. ✅ Detecta as novas classes
2. ✅ Registra com as interfaces corretas
3. ✅ Aplica o lifetime apropriado (Scoped)
4. ✅ Mantém a arquitetura limpa

Esta abordagem elimina uma das tarefas mais repetitivas e propensas a erro no desenvolvimento .NET, permitindo que os desenvolvedores se concentrem na lógica de negócio.