namespace workpoint.Domain.Entities;

public class Space
{
    public int Id { get; set; }
    public int CategorieId { get; set; }
    public int MaxCapacity { get; set; }
    public string SpaceName { get; set; }
    public int BranchId { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int? UserId { get; set; }        // when an User posts a Space
    public ICollection<Photo> Photos { get; set; }
    
    // Relations
    public User User { get; set; }
    public Categorie Categorie { get; set; }
    public Branch Branch { get; set; }
    
    // Inverse Relations:
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();


}