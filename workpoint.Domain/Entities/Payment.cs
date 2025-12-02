namespace workpoint.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Total { get; set; }
    public bool PaymentDone { get; set; }
    
    // Relation:
    public Booking Booking { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    
}