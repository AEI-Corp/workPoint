namespace workpoint.Domain.Entities;

public class Images
{
    public int Id { get; set; }
    public int? SpaceId { get; set; }
    public int? BranchId { get; set; }
    public string UrlImage { get; set; }
    
    // Relations
    public Space Space { get; set; }
    public Branch Branch { get; set; }
}