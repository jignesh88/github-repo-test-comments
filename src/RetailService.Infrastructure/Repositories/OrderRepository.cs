using Microsoft.EntityFrameworkCore;
using RetailService.Core.Entities;
using RetailService.Core.Interfaces;
using RetailService.Infrastructure.Data;

namespace RetailService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RetailDbContext _context;

    public OrderRepository(RetailDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        // Potential N+1 query issue here
        return await _context.Orders.FindAsync(id);
    }

    public async Task<List<Order>> GetAllAsync()
    {
        // Missing pagination - could be performance issue
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        order.Id = Guid.NewGuid();
        order.OrderDate = DateTime.UtcNow;

        // Calculate total - should this be here or in domain logic?
        decimal total = 0;
        foreach (var item in order.Items)
        {
            total += item.Price * item.Quantity;
        }
        order.TotalAmount = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}
