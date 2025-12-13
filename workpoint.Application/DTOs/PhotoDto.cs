using Microsoft.AspNetCore.Http;

namespace workpoint.Application.DTOs;

public class PhotoResponseDto
{
    public int Id { get; set; }
    public int? SpaceId { get; set; }

    public string? UrlImage { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PhotoAddDto
{
    public IFormFile Photo { get; set; }  // Cambiar de Stream a IFormFile
    public int? SpaceId { get; set; }
}