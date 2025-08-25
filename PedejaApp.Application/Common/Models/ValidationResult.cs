namespace PedejaApp.Application.Common.Models;

/// <summary>
/// Resultado de uma operação de validação
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indica se a validação foi bem-sucedida
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// Mensagem de erro principal (quando IsValid = false)
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// Lista detalhada de erros de validação
    /// </summary>
    public IEnumerable<string> Errors { get; init; } = new List<string>();
    
    /// <summary>
    /// Cria um resultado de validação bem-sucedida
    /// </summary>
    /// <returns>ValidationResult com IsValid = true</returns>
    public static ValidationResult Success() => new() { IsValid = true };
    
    /// <summary>
    /// Cria um resultado de validação com falha
    /// </summary>
    /// <param name="errorMessage">Mensagem de erro principal</param>
    /// <returns>ValidationResult com IsValid = false</returns>
    public static ValidationResult Failure(string errorMessage) => new() 
    { 
        IsValid = false, 
        ErrorMessage = errorMessage 
    };
    
    /// <summary>
    /// Cria um resultado de validação com falha e lista de erros
    /// </summary>
    /// <param name="errorMessage">Mensagem de erro principal</param>
    /// <param name="errors">Lista de erros detalhados</param>
    /// <returns>ValidationResult com IsValid = false</returns>
    public static ValidationResult Failure(string errorMessage, IEnumerable<string> errors) => new() 
    { 
        IsValid = false, 
        ErrorMessage = errorMessage,
        Errors = errors 
    };
    
    /// <summary>
    /// Cria um resultado de validação com falha baseado em lista de erros
    /// </summary>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ValidationResult com IsValid = false</returns>
    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorList.FirstOrDefault() ?? "Erro de validação",
            Errors = errorList
        };
    }
}