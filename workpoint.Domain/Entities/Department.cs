using System.Numerics;

namespace workpoint.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string DepartmentName { get; set; }
    public bool Active { get; set; }
    
    // Inverse Relation:
    public ICollection<City> Cities { get; set; } = new List<City>();
}