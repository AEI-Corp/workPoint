using workpoint.Domain.Entities;

namespace workpoint.Application.DTOs;

public class SpaceCreateDto
{
    public int CategorieId { get; set; }
    public int MaxCapacity { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }                    // when an User posts a Space
    // public ICollection<Photo>? Photos { get; set; }      // List of photos
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } 
}


public class SpaceUpdateDto
{
    public int Id { get; set; }
    public int CategorieId { get; set; }
    public int MaxCapacity { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }                    // when an User posts a Space
    public ICollection<Photo> Photos { get; set; }      // List of photos
}

public class ResponseSpaceDto
{
    public int Id { get; set; }
    public int CategorieId { get; set; }
    public int MaxCapacity { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public List<PhotoResponseDto> Photos { get; set; } 
}