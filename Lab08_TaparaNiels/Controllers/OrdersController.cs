using Lab08_TaparaNiels.Models;

namespace Lab08_TaparaNiels.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Lab08_TaparaNiels.Data;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    //Ejercicio 4
    [HttpGet("{orderId}/cantidad-total")]
    public async Task<ActionResult<int>> GetTotalProductQuantity(int orderId)
    {
        var totalQuantity = await _context.Orderdetails
            .Where(od => od.OrderId == orderId)
            .SumAsync(od => od.Quantity);

        return totalQuantity;
    }
    
    //Ejercicio 6
    [HttpGet("despues-de/{date}")]
    public async Task<ActionResult<IEnumerable<object>>> GetOrdersAfterDate(DateTime date)
    {
        var orders = await _context.Orders
            .Where(o => o.OrderDate > date)
            .OrderBy(o => o.OrderDate)
            .Select(o => new 
            {
                o.OrderId,
                o.OrderDate,
                ClientName = o.Client.Name,
                ClientEmail = o.Client.Email
            })
            .ToListAsync();

        if (!orders.Any())
        {
            return NotFound($"No se encontraron pedidos después de {date:yyyy-MM-dd}");
        }

        return Ok(orders);
    }
    
    //Ejercicio 10
    [HttpGet("detalles")]
    public async Task<ActionResult<IEnumerable<object>>> GetOrdersWithDetails()
    {
        var ordersWithDetails = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Orderdetails)
            .ThenInclude(od => od.Product)
            .Select(o => new
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                ClientName = o.Client.Name,
                Details = o.Orderdetails.Select(od => new
                {
                    ProductName = od.Product.Name,
                    od.Quantity,
                    UnitPrice = od.Product.Price,
                    TotalPrice = od.Quantity * od.Product.Price
                }),
                OrderTotal = o.Orderdetails.Sum(od => od.Quantity * od.Product.Price)
            })
            .ToListAsync();

        return Ok(ordersWithDetails);
    }
}