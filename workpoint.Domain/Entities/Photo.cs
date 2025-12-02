namespace workpoint.Domain.Entities;

public class Photo
{
    public int Id { get; set; }
    public int? SpaceId { get; set; }
    public int? BranchId { get; set; }
    public string UrlImage { get; set; }
    public bool Active { get; set; }
    
    // Relations
    public Space Space { get; set; }
    public Branch Branch { get; set; }
}