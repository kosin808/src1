using WebApplication1.Models_DTOs;

namespace WebApplication1.Services;

public interface IDBService
{
    Task<DeliveryInfoDTO> GetDeliveryInfo(int id);

    Task AddDelivery(AddDeliveryDTO delivery);
}