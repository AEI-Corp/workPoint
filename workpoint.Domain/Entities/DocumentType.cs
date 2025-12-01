namespace workpoint.Domain.Entities;

public class DocumentType
{
    public int Id { get; set; }
    public string DocType { get; set; }
    public bool Active { get; set; }
    
    // Inverse Relation:
    public ICollection<User> Users { get; set; } = new List<User>();
}