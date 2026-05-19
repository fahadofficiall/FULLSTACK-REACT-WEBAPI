using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsApi.DTOs;
using ProductsApi.Interfaces;
using ProductsApi.Models;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]                          // all endpoints require a valid JWT
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;

    public ProductsController(IProductRepository repo)
    {
        _repo = repo;
    }

    // ── Helper: Model → DTO ───────────────────────────────────────────────────
    private static ProductDto ToDto(Product p) => new()
    {
        Id          = p.Id,
        Name        = p.Name,
        Description = p.Description,
        Price       = p.Price,
        Stock       = p.Stock,
        Category    = p.Category,
        CreatedAt   = p.CreatedAt,
        UpdatedAt   = p.UpdatedAt
    };

    // ── GET api/products ──────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repo.GetAllAsync();
        return Ok(products.Select(ToDto));
    }

    // ── GET api/products/{id} ─────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product {id} not found." });

        return Ok(ToDto(product));
    }

    // ── POST api/products ─────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Product name is required." });

        if (dto.Price < 0)
            return BadRequest(new { message = "Price must be >= 0." });

        if (dto.Stock < 0)
            return BadRequest(new { message = "Stock must be >= 0." });

        var product = new Product
        {
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price       = dto.Price,
            Stock       = dto.Stock,
            Category    = dto.Category?.Trim()
        };

        var newId = await _repo.CreateAsync(product);
        var created = await _repo.GetByIdAsync(newId);

        return CreatedAtAction(nameof(GetById), new { id = newId }, ToDto(created!));
    }

    // ── PUT api/products/{id} ─────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!await _repo.ExistsAsync(id))
            return NotFound(new { message = $"Product {id} not found." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Product name is required." });

        if (dto.Price < 0)
            return BadRequest(new { message = "Price must be >= 0." });

        if (dto.Stock < 0)
            return BadRequest(new { message = "Stock must be >= 0." });

        var product = new Product
        {
            Id          = id,
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price       = dto.Price,
            Stock       = dto.Stock,
            Category    = dto.Category?.Trim()
        };

        var updated = await _repo.UpdateAsync(product);
        if (!updated)
            return StatusCode(500, new { message = "Update failed." });

        var result = await _repo.GetByIdAsync(id);
        return Ok(ToDto(result!));
    }

    // ── DELETE api/products/{id} ──────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.ExistsAsync(id))
            return NotFound(new { message = $"Product {id} not found." });

        var deleted = await _repo.DeleteAsync(id);
        if (!deleted)
            return StatusCode(500, new { message = "Delete failed." });

        return NoContent();
    }
}
