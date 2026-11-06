using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Models;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public ProductsController(InventoryDbContext db)
    {
        _db = db;
    }

    // GET: api/products
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products.ToListAsync();
        return Ok(products);
    }

    // GET: api/products/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) ||
            string.IsNullOrWhiteSpace(product.SKU))
            return BadRequest("Name and SKU are required.");

        if (product.Quantity < 0 ||
            product.MinStockLevel < 0 ||
            product.Price < 0)
            return BadRequest("Values cannot be negative.");

        if (await _db.Products.AnyAsync(p => p.SKU == product.SKU))
            return Conflict("SKU already exists.");

        product.Id = 0;
        product.CreatedAt = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id, Product updatedProduct)
    {
        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(updatedProduct.Name) ||
            string.IsNullOrWhiteSpace(updatedProduct.SKU))
            return BadRequest("Name and SKU are required.");

        if (updatedProduct.Quantity < 0 ||
            updatedProduct.MinStockLevel < 0 ||
            updatedProduct.Price < 0)
            return BadRequest("Values cannot be negative.");

        if (await _db.Products.AnyAsync(
            p => p.SKU == updatedProduct.SKU && p.Id != id))
            return Conflict("SKU already exists.");

        product.Name = updatedProduct.Name;
        product.SKU = updatedProduct.SKU;
        product.Description = updatedProduct.Description;
        product.MinStockLevel = updatedProduct.MinStockLevel;
        product.Price = updatedProduct.Price;

        await _db.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        if (await _db.StockTransactions.AnyAsync(
            t => t.ProductId == id))
            return Conflict("Product has transaction history.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/products/low-stock
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var products = await _db.Products
            .Where(p => p.Quantity <= p.MinStockLevel)
            .ToListAsync();

        return Ok(products);
    }

    // POST: api/products/5/stock-in
    [HttpPost("{id:int}/stock-in")]
    public async Task<IActionResult> StockIn(
        int id, StockMovementRequest request)
    {
        return await MoveStock(id, request.Quantity, true);
    }

    // POST: api/products/5/stock-out
    [HttpPost("{id:int}/stock-out")]
    public async Task<IActionResult> StockOut(
        int id, StockMovementRequest request)
    {
        return await MoveStock(id, request.Quantity, false);
    }

    // Shared stock movement logic
    private async Task<IActionResult> MoveStock(
        int id, int quantity, bool isStockIn)
    {
        if (quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        if (!isStockIn && quantity > product.Quantity)
            return BadRequest("Insufficient stock.");

        product.Quantity += isStockIn ? quantity : -quantity;

        _db.StockTransactions.Add(new StockTransaction
        {
            ProductId = id,
            Quantity = quantity,
            TransactionType = isStockIn ? "IN" : "OUT"
        });

        await _db.SaveChangesAsync();

        return Ok(product);
    }

    // GET: api/products/5/transactions
    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id))
            return NotFound("Product not found.");

        var transactions = await _db.StockTransactions
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(transactions);
    }
}

public class StockMovementRequest
{
    public int Quantity { get; set; }
}