namespace workpoint.Domain.Entities;

public class Branch
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string BranchName { get; set; }
    public string Address { get; set; }
    public bool Active { get; set; }
    public ICollection<Photo> Photos { get; set; }
    
    // Inverse Relation:
    public ICollection<Space> Spaces { get; set; } = new List<Space>();

} 