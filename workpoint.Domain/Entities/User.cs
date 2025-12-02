namespace workpoint.Domain.Entities;

public class User
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public string LastName { get; set; }
    public int DocumentTypeId { get; set; }     // NEW
    public string NumDocument { get; set; }
    
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpire { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    
    
    // Relation:
    public Role Role { get; set; }
    public DocumentType DocumentType { get; set; }
    
    // Inverse Relations:
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}