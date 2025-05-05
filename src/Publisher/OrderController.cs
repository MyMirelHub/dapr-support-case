using Microsoft.AspNetCore.Mvc;
using Dapr.Client;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly DaprClient _daprClient;
    private const string PUBSUB_NAME = "order-pubsub";
    private const string TOPIC_NAME = "order";

    public OrderController(ILogger<OrderController> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder(Order order)
    {
        try
        {
            _logger.LogInformation("Publishing order: {OrderId}", order.Id);
            await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, order);
            return Ok(new { OrderId = order.Id, Message = "Order submitted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing order: {OrderId}", order.Id);
            return StatusCode(500, new { Message = "Error processing order" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Healthy");
    }
}