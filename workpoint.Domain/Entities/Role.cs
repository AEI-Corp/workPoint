namespace workpoint.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool Active { get; set; }
    
    // Inverse Relation:
    public ICollection<User> Users { get; set; } = new List<User>();
}