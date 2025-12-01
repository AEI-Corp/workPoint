namespace workpoint.Domain.Entities;

public class PaymentMethod
{
    public int Id { get; set; }
    public string PaymentMethodName { get; set; }
    public bool Active { get; set; }
    
    // Inverse Relation:
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}