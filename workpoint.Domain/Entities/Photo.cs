namespace workpoint.Domain.Entities;

public class Photo
{
    public int Id { get; set; }
    public int? SpaceId { get; set; }
    public int? BranchId { get; set; }
    public string? UrlImage { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Relations
    public Space Space { get; set; }
    public Branch Branch { get; set; }
}