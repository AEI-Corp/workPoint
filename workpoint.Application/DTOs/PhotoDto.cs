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
    public Stream Photo { get; set; }
    public int? SpaceId { get; set; }
}