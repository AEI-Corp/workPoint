namespace workpoint.Domain.Entities;

public class Branch
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string BranchName { get; set; }
    public string Address { get; set; }
    public bool Active { get; set; }
    
    //TODO:
    // public List<string?> UrlImagesBranches { get; set; }
    public List<Image> Images { get; set; } = new();
} 