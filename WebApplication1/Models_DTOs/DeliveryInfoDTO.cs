namespace WebApplication1.Models_DTOs;

public class DeliveryInfoDTO
{
    public DateTime Date { get; set; }
    public Customer Customer { get; set; }
    public Driver Driver { get; set; }
    public List<Product> Products { get; set; }
}

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class Driver
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenceNumber { get; set; }
}

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
}