using Microsoft.AspNetCore.Mvc;
using RetailService.Core.Entities;
using RetailService.Core.Interfaces;

namespace RetailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrdersController(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
        // Missing validation - what if items are empty?
        // No stock checking before creating order
        // No transaction handling

        var createdOrder = await _orderRepository.CreateAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Order>> Update(Guid id, [FromBody] Order order)
    {
        var existing = await _orderRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        order.Id = id;
        var updated = await _orderRepository.UpdateAsync(order);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _orderRepository.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
