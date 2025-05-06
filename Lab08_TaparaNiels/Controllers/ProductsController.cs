using Lab08_TaparaNiels.Models;

namespace Lab08_TaparaNiels.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab08_TaparaNiels.Data;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    //Ejercicio 2
    [HttpGet("mayores-a/{minPrice}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsMoreExpensiveThan(decimal minPrice)
    {
        var products = await _context.Products
            .Where(p => p.Price > minPrice)
            .ToListAsync();

        if (!products.Any())
        {
            return NotFound($"No hay productos con precio mayor a {minPrice}");
        }

        return products;
    }
    
    //Ejercicio 5
    [HttpGet("product-mas-caro")]
    public async Task<ActionResult<Product>> GetMostExpensiveProduct()
    {
        var mostExpensiveProduct = await _context.Products
            .OrderByDescending(p => p.Price)
            .FirstOrDefaultAsync();
        
        return mostExpensiveProduct;
    }
    
    //Ejercicio 7
    [HttpGet("promedio-precios")]
    public async Task<ActionResult<decimal>> GetAverageProductPrice()
    {
        var averagePrice = await _context.Products
            .AverageAsync(p => p.Price);

        return Ok(Math.Round(averagePrice, 2));
    }
    
    //Ejercicio 8
    [HttpGet("sin-descripcion")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsWithoutDescription()
    {
        var products = await _context.Products
            .Where(p => string.IsNullOrEmpty(p.Description))
            .ToListAsync();
        
        return products;
    }
    
    //Ejercicio 12
    [HttpGet("{productId}/buscar-clientes")]
    public async Task<ActionResult<IEnumerable<object>>> GetClientsByPurchasedProduct(int productId)
    {
        if (!await _context.Products.AnyAsync(p => p.ProductId == productId))
        {
            return NotFound($"Producto con ID {productId} no encontrado");
        }

        var clients = await _context.Orderdetails
            .Where(od => od.ProductId == productId)
            .Select(od => new
            {
                od.Order.Client.ClientId,
                od.Order.Client.Name,
                od.Order.Client.Email,
                PurchaseDate = od.Order.OrderDate,
                QuantityPurchased = od.Quantity,
                TotalSpent = od.Quantity * od.Product.Price
            })
            .Distinct()
            .ToListAsync();
        
        var productName = await _context.Products
            .Where(p => p.ProductId == productId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync();

        var summary = new
        {
            ProductId = productId,
            ProductName = productName,
            TotalClients = clients.Count,
            TotalQuantitySold = clients.Sum(c => c.QuantityPurchased),
            TotalRevenue = clients.Sum(c => c.TotalSpent),
            Clients = clients
        };

        return Ok(summary);
    }
}