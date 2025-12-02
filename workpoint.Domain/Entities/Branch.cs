namespace workpoint.Domain.Entities;

public class Branch
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool Active { get; set; }
    public ICollection<Photo> Photos { get; set; }      // List of photos
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Inverse Relation:
    public ICollection<Space> Spaces { get; set; } = new List<Space>();
    
    // Relations:
    public City City { get; set; }

} 