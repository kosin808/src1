namespace WebApplication1.Models_DTOs;

public class AddDeliveryDTO
{
    public int DeliveryId { get; set; }
    public int CustomerID { get; set; }
    public string LicenseNumber { get; set; }
    public List<ProductServiceInput> Services { get; set; } = new List<ProductServiceInput>();
}

public class ProductServiceInput
{
    public string ServiceName { get; set; }
    public int amount { get; set; }
}