using Microsoft.AspNetCore.Mvc;
using Dapr;

[ApiController]
[Route("[controller]")]
public class OrderProcessor : ControllerBase
{
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }

    [HttpPost("process-order")]
    [Topic("order-pubsub", "order")]
    public async Task<IActionResult> ProcessOrder(Order order)
    {
        try
        {
            _logger.LogInformation("Processing order: {OrderId}", order.Id);
            
            // Simulate processing
            foreach (var item in order.Items)
            {
                _logger.LogInformation("Processing item: {ProductId}, Quantity: {Quantity}", 
                    item.ProductId, item.Quantity);
                await Task.Delay(100); // Simulate work
            }
            
            _logger.LogInformation("Order processed successfully: {OrderId}", order.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Healthy");
    }
}