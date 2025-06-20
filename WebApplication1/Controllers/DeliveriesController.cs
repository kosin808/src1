using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models_DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeliveriesController : ControllerBase
{
    private readonly IDBService _idbService;

    public DeliveriesController(IDBService idDbService)
    {
        _idbService = idDbService;
    }
    // api/appointments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDelivery(int id)
    {
        try
        {
            var result = await _idbService.GetDeliveryInfo(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    // api/deliveries
    [HttpPost]
    public async Task<IActionResult> AddDelivery([FromBody] AddDeliveryDTO delivery)
    {
        if (!delivery.Products.Any())
            return BadRequest("Bad input");
        
        try
        {
            await _idbService.AddDelivery(delivery);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException e)
        {
            return Conflict(e.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
        return CreatedAtAction(nameof(GetDelivery), new { id = delivery.DeliveryId }, delivery);
    }
    
}