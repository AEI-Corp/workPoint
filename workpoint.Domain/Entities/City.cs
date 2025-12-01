namespace workpoint.Domain.Entities;

public class City
{
    public int Id { get; set; }
    public string CityName { get; set; }
    public int DepartmentId { get; set; }
    public bool Active { get; set; }
    
    // Relations:
    public Department Department { get; set; }
    
    // Inverse relation:
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}