using System.ComponentModel.DataAnnotations;

namespace PedejaApp.Application.Features.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateCategoryDto
{
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Description { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }
}

public class ReorderCategoriesDto
{
    public Dictionary<int, int> CategoryDisplayOrders { get; set; } = new();
}

public class CategoryReorderItem
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
}