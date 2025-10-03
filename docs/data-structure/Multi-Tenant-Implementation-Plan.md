# Plano de Implementação Multi-Tenant - PedejaApp

## 🎯 Objetivo

Transformar o sistema `pedeja-api` de arquitetura single-tenant para uma plataforma SaaS multi-tenant robusta, permitindo que múltiplos clientes (restaurantes) tenham seus próprios dados isolados, mantendo eficiência de recursos e facilidade de manutenção.

## 📋 Visão Geral da Arquitetura

### **Estratégia Escolhida: Schema Compartilhado com Tenant ID**

A arquitetura implementará um modelo híbrido onde:
- **Banco único** com schema compartilhado
- **Isolamento por `tenant_id`** em todas as tabelas principais
- **Row Level Security (RLS)** do PostgreSQL para segurança automática
- **Estrutura preparada** para múltiplos produtos (delivery, cardápio digital, etc.)

### **Vantagens da Abordagem**
✅ **Eficiência de recursos** - Um banco para todos os tenants  
✅ **Manutenção simplificada** - Migrations e backups unificados  
✅ **Escalabilidade** - Suporte a milhares de tenants  
✅ **Segurança** - RLS impede vazamento de dados entre tenants  
✅ **Flexibilidade** - Preparado para múltiplos produtos  

## 🏗️ Estrutura Multi-Tenant Definida

### **Entidades Principais do Sistema**

#### 1. **Tenants (Organizações/Restaurantes)**
```sql
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,                    -- Nome do restaurante
    slug VARCHAR(100) UNIQUE NOT NULL,             -- URL amigável (ex: pizzaria-bella)
    domain VARCHAR(255),                           -- Domínio personalizado opcional
    status VARCHAR(20) DEFAULT 'active',           -- active, suspended, cancelled
    settings JSONB DEFAULT '{}',                   -- Configurações específicas
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE            -- Soft delete
);
```

#### 2. **Products (Produtos/Aplicações da Plataforma)**
```sql
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,                    -- Ex: "Sistema de Delivery"
    slug VARCHAR(100) UNIQUE NOT NULL,             -- Ex: "delivery-system"
    description TEXT,
    category VARCHAR(100) NOT NULL,                -- Ex: "food-delivery"
    version VARCHAR(20) DEFAULT '1.0.0',
    status VARCHAR(20) DEFAULT 'active',
    configuration_schema JSONB DEFAULT '{}',       -- Schema de configurações
    pricing_model VARCHAR(50) DEFAULT 'subscription',
    base_price DECIMAL(10,2) DEFAULT 0.00,
    setup_fee DECIMAL(10,2) DEFAULT 0.00,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

#### 3. **Plans (Planos de Assinatura)**
```sql
CREATE TABLE plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,                    -- Ex: "Plano Premium"
    slug VARCHAR(100) UNIQUE NOT NULL,             -- Ex: "premium"
    description TEXT,
    price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    billing_cycle VARCHAR(20) DEFAULT 'monthly',   -- monthly, quarterly, yearly
    max_users INTEGER DEFAULT 1,
    max_storage_gb INTEGER DEFAULT 1,
    features JSONB DEFAULT '[]',                   -- Array de funcionalidades
    status VARCHAR(20) DEFAULT 'active',
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

#### 4. **Subscriptions (Assinaturas)**
```sql
CREATE TABLE subscriptions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    product_id UUID NOT NULL REFERENCES products(id),
    plan_id UUID NOT NULL REFERENCES plans(id),
    status VARCHAR(20) DEFAULT 'active',           -- active, cancelled, expired, suspended, trial
    trial_ends_at TIMESTAMP WITH TIME ZONE,
    current_period_start TIMESTAMP WITH TIME ZONE NOT NULL,
    current_period_end TIMESTAMP WITH TIME ZONE NOT NULL,
    cancel_at_period_end BOOLEAN DEFAULT FALSE,
    cancelled_at TIMESTAMP WITH TIME ZONE,
    custom_pricing DECIMAL(10,2),                  -- Preço customizado se aplicável
    usage_limits JSONB DEFAULT '{}',               -- Limites específicos
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, product_id)                  -- Um produto por tenant
);
```

#### 5. **Users (Usuários do Sistema)**
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),        -- NULL para admins da plataforma
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    avatar_url VARCHAR(500),
    status VARCHAR(20) DEFAULT 'active',
    email_verified_at TIMESTAMP WITH TIME ZONE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    preferences JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE
);
```

### **Sistema de Permissões (RBAC)**

#### 6. **Roles (Papéis/Funções)**
```sql
CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),        -- NULL para roles do sistema
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    description TEXT,
    is_system_role BOOLEAN DEFAULT FALSE,         -- TRUE para roles globais
    is_default BOOLEAN DEFAULT FALSE,             -- Role padrão para novos usuários
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(tenant_id, slug)
);
```

#### 7. **Permissions (Permissões)**
```sql
CREATE TABLE permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID REFERENCES products(id),      -- Permissão específica do produto
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    description TEXT,
    resource VARCHAR(100) NOT NULL,               -- Ex: "products", "orders"
    action VARCHAR(50) NOT NULL,                  -- Ex: "create", "read", "update", "delete"
    scope VARCHAR(50) DEFAULT 'tenant',           -- global, tenant, user
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(product_id, slug),
    UNIQUE(product_id, resource, action)
);
```

### **Auditoria Completa**

#### 8. **Audit Logs**
```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(id),
    user_id UUID REFERENCES users(id),
    action VARCHAR(50) NOT NULL,                   -- CREATE, UPDATE, DELETE
    resource_type VARCHAR(100) NOT NULL,          -- Nome da tabela
    resource_id UUID,                             -- ID do recurso afetado
    old_values JSONB,                             -- Valores anteriores
    new_values JSONB,                             -- Novos valores
    ip_address INET,                              -- IP do usuário
    user_agent TEXT,                              -- Browser/App info
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
) PARTITION BY RANGE (created_at);               -- Particionamento por data
```

## 🔄 Adaptação das Tabelas Existentes

### **Tabelas que Receberão `tenant_id`:**

#### Categories
```sql
ALTER TABLE "Categories" ADD COLUMN tenant_id UUID REFERENCES tenants(id);
CREATE INDEX idx_categories_tenant_id ON "Categories"(tenant_id);
```

#### Products  
```sql
ALTER TABLE "Products" ADD COLUMN tenant_id UUID REFERENCES tenants(id);
CREATE INDEX idx_products_tenant_id ON "Products"(tenant_id);
```

#### Orders
```sql
ALTER TABLE "Orders" ADD COLUMN tenant_id UUID REFERENCES tenants(id);
CREATE INDEX idx_orders_tenant_id ON "Orders"(tenant_id);
```

#### AddonGroups
```sql
ALTER TABLE "AddonGroups" ADD COLUMN tenant_id UUID REFERENCES tenants(id);
CREATE INDEX idx_addon_groups_tenant_id ON "AddonGroups"(tenant_id);
```

#### Addons
```sql
ALTER TABLE "Addons" ADD COLUMN tenant_id UUID REFERENCES tenants(id);
CREATE INDEX idx_addons_tenant_id ON "Addons"(tenant_id);
```

### **Row Level Security (RLS)**

#### Políticas Automáticas de Segurança
```sql
-- Categories
ALTER TABLE "Categories" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_categories_policy ON "Categories"
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

-- Products
ALTER TABLE "Products" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_products_policy ON "Products" 
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

-- Orders
ALTER TABLE "Orders" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_orders_policy ON "Orders"
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

-- AddonGroups
ALTER TABLE "AddonGroups" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_addon_groups_policy ON "AddonGroups"
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);

-- Addons  
ALTER TABLE "Addons" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_addons_policy ON "Addons"
    USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

## 🛠️ Implementação na Aplicação .NET

### **1. Novas Entidades do Domain**

#### **Tenant.cs**
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Representa uma organização/restaurante na plataforma
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Nome da organização/restaurante
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único para URLs (ex: pizzaria-bella)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Domínio personalizado opcional
    /// </summary>
    [MaxLength(255)]
    public string? Domain { get; set; }

    /// <summary>
    /// Status do tenant (active, suspended, cancelled)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Configurações específicas do tenant em JSON
    /// </summary>
    public string Settings { get; set; } = "{}";

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

#### **Product.cs** (Produto da Plataforma)
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Representa um produto tecnológico oferecido pela plataforma (ex: Sistema de Delivery)
/// </summary>
public class PlatformProduct : BaseEntity
{
    /// <summary>
    /// Nome do produto (ex: Sistema de Delivery)
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Slug único para o produto
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do produto
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Categoria do produto (food-delivery, accounting, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Versão atual do produto
    /// </summary>
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Status do produto
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Schema de configurações em JSON
    /// </summary>
    public string ConfigurationSchema { get; set; } = "{}";

    /// <summary>
    /// Modelo de precificação
    /// </summary>
    [MaxLength(50)]
    public string PricingModel { get; set; } = "subscription";

    /// <summary>
    /// Preço base do produto
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal BasePrice { get; set; } = 0.00m;

    /// <summary>
    /// Taxa de configuração inicial
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal SetupFee { get; set; } = 0.00m;

    // Navigation properties
    public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
```

#### **Subscription.cs**
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Representa uma assinatura de um tenant para um produto específico
/// </summary>
public class Subscription : BaseEntity
{
    /// <summary>
    /// ID do tenant que possui a assinatura
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// ID do produto assinado
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// ID do plano escolhido
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Status da assinatura
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Data de término do período trial (se aplicável)
    /// </summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>
    /// Início do período atual da assinatura
    /// </summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>
    /// Fim do período atual da assinatura
    /// </summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>
    /// Se deve cancelar no fim do período atual
    /// </summary>
    public bool CancelAtPeriodEnd { get; set; } = false;

    /// <summary>
    /// Data do cancelamento (se cancelado)
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Preço customizado (se diferente do plano)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? CustomPricing { get; set; }

    /// <summary>
    /// Limites específicos de uso em JSON
    /// </summary>
    public string UsageLimits { get; set; } = "{}";

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual PlatformProduct Product { get; set; } = null!;
    public virtual Plan Plan { get; set; } = null!;
}
```

### **2. Adaptação das Entidades Existentes**

#### **BaseEntity.cs** (Atualizada)
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Classe base para todas as entidades do sistema
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único da entidade
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Data de criação da entidade
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

#### **TenantEntity.cs** (Nova classe base)
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Classe base para entidades que pertencem a um tenant específico
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    /// <summary>
    /// ID do tenant proprietário desta entidade
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navegação para o tenant
    /// </summary>
    public virtual Tenant Tenant { get; set; } = null!;
}
```

#### **Category.cs** (Atualizada)
```csharp
namespace PedejaApp.Domain.Entities;

/// <summary>
/// Categoria de produtos de um tenant específico
/// </summary>
public class Category : TenantEntity
{
    /// <summary>
    /// Nome da categoria
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da categoria
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Ordem de exibição
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Se a categoria está ativa
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
```

### **3. Serviços de Tenant**

#### **ITenantService.cs**
```csharp
namespace PedejaApp.Application.Common.Interfaces;

/// <summary>
/// Serviço para gerenciar o contexto do tenant atual
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// ID do tenant atual
    /// </summary>
    Guid? CurrentTenantId { get; }

    /// <summary>
    /// Slug do tenant atual
    /// </summary>
    string? CurrentTenantSlug { get; }

    /// <summary>
    /// Define o tenant atual por ID
    /// </summary>
    void SetCurrentTenant(Guid tenantId);

    /// <summary>
    /// Define o tenant atual por slug
    /// </summary>
    Task<bool> SetCurrentTenantBySlugAsync(string slug);

    /// <summary>
    /// Obtém informações do tenant atual
    /// </summary>
    Task<TenantDto?> GetCurrentTenantAsync();

    /// <summary>
    /// Verifica se o tenant atual tem acesso ao produto
    /// </summary>
    Task<bool> HasProductAccessAsync(string productSlug);

    /// <summary>
    /// Limpa o contexto do tenant atual
    /// </summary>
    void ClearCurrentTenant();
}
```

#### **TenantService.cs**
```csharp
namespace PedejaApp.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de tenant
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantService> _logger;
    private readonly IMemoryCache _cache;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        ITenantRepository tenantRepository,
        ILogger<TenantService> logger,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantRepository = tenantRepository;
        _logger = logger;
        _cache = cache;
    }

    public Guid? CurrentTenantId => 
        _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;

    public string? CurrentTenantSlug =>
        _httpContextAccessor.HttpContext?.Items["TenantSlug"] as string;

    public void SetCurrentTenant(Guid tenantId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items["TenantId"] = tenantId;
            _logger.LogDebug("Tenant atual definido: {TenantId}", tenantId);
        }
    }

    public async Task<bool> SetCurrentTenantBySlugAsync(string slug)
    {
        var cacheKey = $"tenant_slug_{slug}";
        
        if (!_cache.TryGetValue(cacheKey, out TenantDto? tenant))
        {
            tenant = await _tenantRepository.GetBySlugAsync(slug);
            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(30));
            }
        }

        if (tenant?.Status == "active")
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantSlug"] = tenant.Slug;
                _logger.LogDebug("Tenant atual definido por slug: {Slug} -> {TenantId}", slug, tenant.Id);
            }
            return true;
        }

        return false;
    }

    public async Task<TenantDto?> GetCurrentTenantAsync()
    {
        if (CurrentTenantId == null) return null;

        var cacheKey = $"tenant_{CurrentTenantId}";
        
        if (!_cache.TryGetValue(cacheKey, out TenantDto? tenant))
        {
            tenant = await _tenantRepository.GetByIdAsync(CurrentTenantId.Value);
            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(30));
            }
        }

        return tenant;
    }

    public async Task<bool> HasProductAccessAsync(string productSlug)
    {
        if (CurrentTenantId == null) return false;

        return await _tenantRepository.HasActiveSubscriptionAsync(CurrentTenantId.Value, productSlug);
    }

    public void ClearCurrentTenant()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Items.Remove("TenantId");
            context.Items.Remove("TenantSlug");
        }
    }
}
```

### **4. Middleware de Detecção de Tenant**

#### **TenantResolutionMiddleware.cs**
```csharp
namespace PedejaApp.Web.Middleware;

/// <summary>
/// Middleware para detectar e definir o tenant atual baseado na requisição
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
        
        // Estratégia 1: Tenant por subdomínio (ex: restaurante-abc.pedeja.com)
        var host = context.Request.Host.Host;
        if (host.Contains('.') && !host.StartsWith("www") && !host.StartsWith("api"))
        {
            var tenantSlug = host.Split('.')[0];
            var tenantFound = await tenantService.SetCurrentTenantBySlugAsync(tenantSlug);
            
            if (tenantFound)
            {
                _logger.LogDebug("Tenant detectado por subdomínio: {TenantSlug}", tenantSlug);
                await _next(context);
                return;
            }
        }

        // Estratégia 2: Tenant por header X-Tenant-Slug
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var tenantHeader))
        {
            var tenantSlug = tenantHeader.ToString();
            var tenantFound = await tenantService.SetCurrentTenantBySlugAsync(tenantSlug);
            
            if (tenantFound)
            {
                _logger.LogDebug("Tenant detectado por header: {TenantSlug}", tenantSlug);
                await _next(context);
                return;
            }
        }

        // Estratégia 3: Tenant por query parameter (para desenvolvimento)
        if (context.Request.Query.TryGetValue("tenant", out var tenantQuery))
        {
            var tenantSlug = tenantQuery.ToString();
            var tenantFound = await tenantService.SetCurrentTenantBySlugAsync(tenantSlug);
            
            if (tenantFound)
            {
                _logger.LogDebug("Tenant detectado por query param: {TenantSlug}", tenantSlug);
                await _next(context);
                return;
            }
        }

        // Se chegou até aqui, nenhum tenant foi detectado
        _logger.LogWarning("Nenhum tenant foi detectado para a requisição: {Path}", context.Request.Path);
        
        // Para APIs, retornar 400 Bad Request
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant não especificado");
            return;
        }

        await _next(context);
    }
}
```

### **5. Configuração do DbContext**

#### **ApplicationDbContext.cs** (Atualizado)
```csharp
namespace PedejaApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    // DbSets da estrutura multi-tenant
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PlatformProduct> PlatformProducts { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // DbSets existentes (agora com tenant_id)
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<AddonGroup> AddonGroups { get; set; }
    public DbSet<Addon> Addons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações das novas entidades
        ConfigureMultiTenantEntities(modelBuilder);
        
        // Configurações das entidades existentes (adicionando tenant_id)
        ConfigureExistingEntities(modelBuilder);
        
        // Configurar query filters para multi-tenancy
        ConfigureQueryFilters(modelBuilder);
    }

    private void ConfigureMultiTenantEntities(ModelBuilder modelBuilder)
    {
        // Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Settings)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        });

        // PlatformProduct
        modelBuilder.Entity<PlatformProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.ConfigurationSchema)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        });

        // Subscription
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ProductId }).IsUnique();
            entity.Property(e => e.UsageLimits)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");

            entity.HasOne(e => e.Tenant)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.TenantId);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.ProductId);
        });

        // Outras configurações...
    }

    private void ConfigureExistingEntities(ModelBuilder modelBuilder)
    {
        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(e => e.Tenant)
                .WithMany(e => e.Categories)
                .HasForeignKey(e => e.TenantId);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(e => e.Tenant)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.TenantId);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(e => e.Tenant)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.TenantId);
        });

        // Outras configurações...
    }

    private void ConfigureQueryFilters(ModelBuilder modelBuilder)
    {
        // Aplicar filtros automáticos por tenant em todas as entidades tenant-específicas
        modelBuilder.Entity<Category>()
            .HasQueryFilter(e => e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<Product>()
            .HasQueryFilter(e => e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<Order>()
            .HasQueryFilter(e => e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<AddonGroup>()
            .HasQueryFilter(e => e.TenantId == _tenantService.CurrentTenantId);

        modelBuilder.Entity<Addon>()
            .HasQueryFilter(e => e.TenantId == _tenantService.CurrentTenantId);
    }

    public override int SaveChanges()
    {
        SetTenantIds();
        SetAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIds();
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantIds()
    {
        var currentTenantId = _tenantService.CurrentTenantId;
        if (currentTenantId == null) return;

        var tenantEntries = ChangeTracker.Entries<TenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in tenantEntries)
        {
            entry.Entity.TenantId = currentTenantId.Value;
        }
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
```

## 📋 Plano de Execução

### **Fase 1: Preparação da Base de Dados (2-3 dias)**
1. ✅ Executar scripts multi-tenant no PostgreSQL
2. ✅ Criar migrations do Entity Framework
3. ✅ Configurar conexões e permissões
4. ✅ Testes básicos de conectividade

### **Fase 2: Implementação das Entidades Core (3-4 dias)**
1. ✅ Criar entidades multi-tenant (Tenant, PlatformProduct, Subscription, etc.)
2. ✅ Adaptar entidades existentes (Category, Product, Order, etc.)
3. ✅ Configurar relacionamentos no DbContext
4. ✅ Implementar query filters automáticos

### **Fase 3: Serviços de Tenant (2-3 dias)**
1. ✅ Implementar ITenantService e TenantService
2. ✅ Criar middleware de detecção de tenant
3. ✅ Configurar injeção de dependências
4. ✅ Implementar cache de informações de tenant

### **Fase 4: Adaptação dos Controladores (3-4 dias)**
1. ✅ Atualizar controllers existentes para usar TenantService
2. ✅ Implementar validações de acesso por produto
3. ✅ Ajustar responses e error handling
4. ✅ Documentar APIs atualizadas

### **Fase 5: Testes e Validação (4-5 dias)**
1. ✅ Testes de isolamento de dados entre tenants
2. ✅ Testes de performance com múltiplos tenants
3. ✅ Validação de segurança (RLS)
4. ✅ Testes de integração completos

### **Fase 6: Migração de Dados (2-3 dias)**
1. ✅ Scripts de migração dos dados existentes
2. ✅ Criação de tenant padrão
3. ✅ Associação de dados existentes ao tenant
4. ✅ Validação da migração

## 🔒 Segurança e Isolamento

### **Row Level Security (RLS)**
- Políticas automáticas por tenant
- Proteção contra queries mal formadas
- Isolamento garantido no nível do banco

### **Validações de Aplicação**
- Verificação de tenant em todos os endpoints
- Middleware de detecção automática
- Cache de informações de tenant

### **Auditoria Completa**
- Log de todas as operações
- Rastreamento por usuário e tenant
- Particionamento de logs por data

## 📊 Produtos Definidos

### **1. Sistema de Delivery** (Implementação Atual)
- **Slug**: `delivery-system`
- **Categoria**: `food-delivery`
- **Preço**: R$ 199,90/mês
- **Funcionalidades**: Gestão de pedidos, cardápio, entregadores

### **2. Cardápio Digital**
- **Slug**: `digital-menu`
- **Categoria**: `food-service`
- **Preço**: R$ 79,90/mês
- **Funcionalidades**: QR Code, personalização, promoções

### **3. Sistema para Madeireira**
- **Slug**: `lumber-system`
- **Categoria**: `lumber-industry`
- **Preço**: R$ 299,90/mês
- **Funcionalidades**: Controle de estoque, otimização de cortes

### **4. Sistema de Contabilidade**
- **Slug**: `accounting-system`
- **Categoria**: `accounting`
- **Preço**: R$ 399,90/mês
- **Funcionalidades**: NFe, SPED, relatórios

### **5. Sistema para Mini Mercado**
- **Slug**: `minimarket-system`
- **Categoria**: `retail`
- **Preço**: R$ 149,90/mês
- **Funcionalidades**: PDV, estoque, fornecedores

## 🚀 Benefícios da Implementação

### **✅ Escalabilidade**
- Suporte a milhares de tenants
- Crescimento horizontal facilitado
- Otimização de recursos

### **✅ Segurança**
- Isolamento total entre tenants
- Auditoria completa
- RLS automático

### **✅ Manutenibilidade**
- Migrations centralizadas
- Backup unificado
- Monitoramento simplificado

### **✅ Flexibilidade**
- Múltiplos produtos na mesma base
- Configurações customizáveis
- Planos flexíveis

### **✅ Economia**
- Compartilhamento de recursos
- Redução de custos operacionais
- Eficiência de desenvolvimento

---

**Status**: 📋 **Planejamento Completo**  
**Data**: Setembro 2025  
**Responsável**: Samuel (Agente de Desenvolvimento)  
**Tecnologias**: PostgreSQL, .NET 9, Entity Framework Core, Multi-Tenant Architecture