namespace workpoint.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int DocumentTypeId { get; set; }                                 // NEW
    public string NumDocument { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; }  = string.Empty;                      // NEW
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    
    public string? RefreshToken { get; set; }  = string.Empty;
    public DateTime? RefreshTokenExpire { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UrlProfilePhoto { get; set; } = string.Empty;
    
    
    
    // Relation:
    public Role Role { get; set; }
    public DocumentType DocumentType { get; set; }
    
    // Inverse Relations:
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}