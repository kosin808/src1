namespace WebApplication1.Models_DTOs;

public class AddDeliveryDTO
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; }
    public List<ProductInput> Products { get; set; }
}

public class ProductInput
{
    public string Name { get; set; }
    public int Amount { get; set; }
}