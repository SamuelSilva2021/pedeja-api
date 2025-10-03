using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Domain.Entities;

/// <summary>
/// Representa uma organização/empresa cliente da plataforma
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Nome da empresa/organização
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identificador amigável para URLs
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
    /// Status da organização (active, suspended, cancelled)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// CNPJ para empresas ou CPF para MEI/pessoa física
    /// </summary>
    [MaxLength(18)]
    public string? CnpjCpf { get; set; }

    /// <summary>
    /// Razão social da empresa (nome oficial)
    /// </summary>
    [MaxLength(255)]
    public string? RazaoSocial { get; set; }

    /// <summary>
    /// Inscrição Estadual
    /// </summary>
    [MaxLength(50)]
    public string? InscricaoEstadual { get; set; }

    /// <summary>
    /// Inscrição Municipal
    /// </summary>
    [MaxLength(50)]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>
    /// Telefone da empresa
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Email corporativo
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Website da empresa
    /// </summary>
    [MaxLength(255)]
    public string? Website { get; set; }

    /// <summary>
    /// Endereço - Logradouro
    /// </summary>
    [MaxLength(255)]
    public string? AddressStreet { get; set; }

    /// <summary>
    /// Endereço - Número
    /// </summary>
    [MaxLength(20)]
    public string? AddressNumber { get; set; }

    /// <summary>
    /// Endereço - Complemento
    /// </summary>
    [MaxLength(100)]
    public string? AddressComplement { get; set; }

    /// <summary>
    /// Endereço - Bairro
    /// </summary>
    [MaxLength(100)]
    public string? AddressNeighborhood { get; set; }

    /// <summary>
    /// Endereço - Cidade
    /// </summary>
    [MaxLength(100)]
    public string? AddressCity { get; set; }

    /// <summary>
    /// Endereço - Estado (UF)
    /// </summary>
    [MaxLength(2)]
    public string? AddressState { get; set; }

    /// <summary>
    /// Endereço - CEP
    /// </summary>
    [MaxLength(10)]
    public string? AddressZipcode { get; set; }

    /// <summary>
    /// Endereço - País
    /// </summary>
    [MaxLength(2)]
    public string AddressCountry { get; set; } = "BR";

    /// <summary>
    /// Endereço de Cobrança - Logradouro
    /// </summary>
    [MaxLength(255)]
    public string? BillingStreet { get; set; }

    /// <summary>
    /// Endereço de Cobrança - Número
    /// </summary>
    [MaxLength(20)]
    public string? BillingNumber { get; set; }

    /// <summary>
    /// Endereço de Cobrança - Complemento
    /// </summary>
    [MaxLength(100)]
    public string? BillingComplement { get; set; }

    /// <summary>
    /// Endereço de Cobrança - Bairro
    /// </summary>
    [MaxLength(100)]
    public string? BillingNeighborhood { get; set; }

    /// <summary>
    /// Endereço de Cobrança - Cidade
    /// </summary>
    [MaxLength(100)]
    public string? BillingCity { get; set; }

    /// <summary>
    /// Endereço de Cobrança - Estado (UF)
    /// </summary>
    [MaxLength(2)]
    public string? BillingState { get; set; }

    /// <summary>
    /// Endereço de Cobrança - CEP
    /// </summary>
    [MaxLength(10)]
    public string? BillingZipcode { get; set; }

    /// <summary>
    /// Endereço de Cobrança - País
    /// </summary>
    [MaxLength(2)]
    public string BillingCountry { get; set; } = "BR";

    /// <summary>
    /// Nome do responsável legal pela empresa
    /// </summary>
    [MaxLength(255)]
    public string? LegalRepresentativeName { get; set; }

    /// <summary>
    /// CPF do responsável legal
    /// </summary>
    [MaxLength(14)]
    public string? LegalRepresentativeCpf { get; set; }

    /// <summary>
    /// Email do responsável legal
    /// </summary>
    [MaxLength(255)]
    public string? LegalRepresentativeEmail { get; set; }

    /// <summary>
    /// Telefone do responsável legal
    /// </summary>
    [MaxLength(20)]
    public string? LegalRepresentativePhone { get; set; }

    /// <summary>
    /// Configurações específicas em formato JSON
    /// </summary>
    public string Settings { get; set; } = "{}";

    /// <summary>
    /// Lista de usuários da organização
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}