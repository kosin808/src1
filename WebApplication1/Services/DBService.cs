using Microsoft.Data.SqlClient;
using WebApplication1.Models_DTOs;

namespace WebApplication1.Services;

public class DBService : IDBService
{
    private readonly string _connectionString;
    
    public DBService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<DeliveryInfoDTO> GetDeliveryInfo(int id)
    {
        string query = @"
            SELECT d.date, c.first_name, c.last_name, c.date_of_birth, r.first_name, r.last_name,r.licence_number, p.name, p.price,pd.amount
            FROM Delivery d 
                INNER JOIN Customer c ON c.customer_id = d.customer_id
                INNER JOIN Driver r ON r.driver_id = d.driver_id
                INNER JOIN Product_Delivery pd ON pd.delivery_id = d.delivery_id
                INNER JOIN Product p ON p.product_id = pd.product_id
                WHERE d.delivery_id = @deliveryId;";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@deliveryId", id);
        var reader = await command.ExecuteReaderAsync();

        DeliveryInfoDTO? deliveryInfo = null;

        while (await reader.ReadAsync())
        {
            if (deliveryInfo is null)
            {
                deliveryInfo = new DeliveryInfoDTO
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    Customer = new Customer
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    },
                    Driver = new Driver
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        LicenceNumber = reader.GetString(reader.GetOrdinal("licence_number")),
                    },
                    Products = new List<Product>(),
                };
            }

            deliveryInfo.Products.Add(
                new Product
                {
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Amount = reader.GetInt32(reader.GetOrdinal("amount"))
                });
        }
        if (deliveryInfo is null)
            throw new ArgumentException("Appointment not found. ");
        
        return deliveryInfo;
    }

    public Task AddDelivery(AddDeliveryDTO delivery)
    {
        throw new NotImplementedException();
    }
}
