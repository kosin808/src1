using System.Data.Common;
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

   public async Task AddDelivery(AddDeliveryDTO delivery)
{
    await using SqlConnection connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    await using var transaction = await connection.BeginTransactionAsync();
    try
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction as SqlTransaction;

        // 1. Znajdź driver_id po licenceNumber
        command.CommandText = "SELECT driver_id FROM Driver WHERE licence_number = @LicenceNumber;";
        command.Parameters.AddWithValue("@LicenceNumber", delivery.LicenceNumber);
        var driverIdObj = await command.ExecuteScalarAsync();
        if (driverIdObj == null)
            throw new ArgumentException($"Driver with licence number {delivery.LicenceNumber} not found.");
        int driverId = (int)driverIdObj;

        // 2. Sprawdź czy customer istnieje
        command.Parameters.Clear();
        command.CommandText = "SELECT 1 FROM Customer WHERE customer_id = @CustomerId;";
        command.Parameters.AddWithValue("@CustomerId", delivery.CustomerId);
        var customerExists = await command.ExecuteScalarAsync();
        if (customerExists == null)
            throw new ArgumentException($"Customer with ID {delivery.CustomerId} not found.");

        // 3. Wstaw nową dostawę do tabeli Delivery
        command.Parameters.Clear();
        command.CommandText = @"
            INSERT INTO Delivery (delivery_id, customer_id, driver_id, date)
            VALUES (@DeliveryId, @CustomerId, @DriverId, @Date);";
        command.Parameters.AddWithValue("@DeliveryId", delivery.DeliveryId);
        command.Parameters.AddWithValue("@CustomerId", delivery.CustomerId);
        command.Parameters.AddWithValue("@DriverId", driverId);
        command.Parameters.AddWithValue("@Date", DateTime.Now);  // lub z JSON, jeśli dodasz pole

        await command.ExecuteNonQueryAsync();

        // 4. Dodaj produkty do Product_Delivery
        foreach (var product in delivery.Products)
        {
            command.Parameters.Clear();

            // Znajdź product_id po nazwie
            command.CommandText = "SELECT product_id FROM Product WHERE name = @ProductName;";
            command.Parameters.AddWithValue("@ProductName", product.Name);
            var productIdObj = await command.ExecuteScalarAsync();
            if (productIdObj == null)
                throw new ArgumentException($"Product '{product.Name}' was not found.");
            int productId = (int)productIdObj;

            // Wstaw do Product_Delivery
            command.Parameters.Clear();
            command.CommandText = @"
                INSERT INTO Product_Delivery (product_id, delivery_id, amount)
                VALUES (@ProductId, @DeliveryId, @Amount);";
            command.Parameters.AddWithValue("@ProductId", productId);
            command.Parameters.AddWithValue("@DeliveryId", delivery.DeliveryId);
            command.Parameters.AddWithValue("@Amount", product.Amount);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}
