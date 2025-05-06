namespace Lab08_TaparaNiels.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab08_TaparaNiels.Data;

[Route("api/[controller]")]
[ApiController]
public class OrderDetailsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Ejercicio 3
    [HttpGet("orden/{orderId}/producto")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductsByOrder(int orderId)
    {
        var productsInOrder = await _context.Orderdetails
            .Where(od => od.OrderId == orderId)
            .Include(od => od.Product)
            .Select(od => new 
            {
                ProductName = od.Product.Name,
                Quantity = od.Quantity,
                UnitPrice = od.Product.Price,
                TotalPrice = od.Quantity * od.Product.Price
            })
            .ToListAsync();

        if (!productsInOrder.Any())
        {
            return NotFound($"No se encontraron productos para la orden {orderId}");
        }

        return productsInOrder;
    }
}