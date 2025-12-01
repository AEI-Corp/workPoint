namespace workpoint.Domain.Entities;

public class Categorie
{
    public int Id { get; set; }
    public string CategorieName { get; set; }
    public bool Active { get; set; }
    
    // Inverse Relation:
    public ICollection<Space> Spaces { get; set; } = new List<Space>();

}