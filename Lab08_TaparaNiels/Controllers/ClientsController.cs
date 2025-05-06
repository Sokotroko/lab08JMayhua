using Lab08_TaparaNiels.Data;
using Lab08_TaparaNiels.Models;

namespace Lab08_TaparaNiels.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    //Ejercicio 1
    [HttpGet("buscar/{name}")]
    public async Task<ActionResult<IEnumerable<Client>>> GetClientsByName(string name)
    {
        var clients = await _context.Clients
            .Where(c => c.Name.Contains(name))
            .ToListAsync();

        if (!clients.Any())
        {
            return NotFound("No se encontraron clientes con ese nombre");
        }

        return clients;
    }
    
    //Ejercicio 9
    [HttpGet("con-mas-pedidos")]
    public async Task<ActionResult<object>> GetClientWithMostOrders()
    {
        var clientWithMostOrders = await _context.Orders
            .GroupBy(o => o.ClientId)
            .Select(g => new {
                ClientId = g.Key,
                OrderCount = g.Count(),
                ClientName = g.First().Client.Name
            })
            .OrderByDescending(x => x.OrderCount)
            .FirstOrDefaultAsync();
        
        return clientWithMostOrders;
    }
    
    //Ejercicio 11
    [HttpGet("{clientId}/productos-vendidos")]
    public async Task<ActionResult<IEnumerable<object>>> GetClientPurchasedProducts(int clientId)
    {
        if (!await _context.Clients.AnyAsync(c => c.ClientId == clientId))
        {
            return NotFound($"Cliente con ID {clientId} no encontrado");
        }

        var products = await _context.Orderdetails
            .Where(od => od.Order.ClientId == clientId)
            .Select(od => new
            {
                od.Product.ProductId,
                od.Product.Name,
                od.Product.Price,
                od.Quantity,
                PurchaseDate = od.Order.OrderDate,
                TotalSpent = od.Quantity * od.Product.Price
            })
            .Distinct()
            .ToListAsync();
        
        var summary = new
        {
            TotalProductsPurchased = products.Sum(p => p.Quantity),
            TotalAmountSpent = products.Sum(p => p.TotalSpent),
            Products = products
        };

        return Ok(summary);
    }
}