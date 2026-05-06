using Microsoft.AspNetCore.Mvc;
using RetailService.Core.DTOs;
using RetailService.Core.Services;

namespace RetailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ProductSearchResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductSearchResultDto>> Search([FromQuery] ProductSearchDto searchDto)
    {
        _logger.LogInformation("Searching products with filters: {@SearchDto}", searchDto);

        // No input validation - should validate PageSize limits
        var result = await _productService.SearchProductsAsync(searchDto);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [Obsolete("Use /search endpoint instead")]
    public async Task<ActionResult<ProductSearchResultDto>> GetAll([FromQuery] string? category = null)
    {
        _logger.LogInformation("Fetching products. Category filter: {Category}", category ?? "None");

        var searchDto = new ProductSearchDto(
            null,
            category,
            null,
            null,
            1,
            100
        );

        var result = await _productService.SearchProductsAsync(searchDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createDto.Name);

        try
        {
            var product = await _productService.CreateProductAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            // Catching all exceptions - too broad!
            _logger.LogError(ex, "Error creating product");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto updateDto)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);

        var product = await _productService.UpdateProductAsync(id, updateDto);
        if (product == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return NotFound();
        }

        return Ok(product);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);

        var result = await _productService.DeleteProductAsync(id);
        if (!result)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        // No validation on quantity - could be huge number
        var result = await _productService.UpdateStockAsync(id, quantity);
        if (!result)
            return NotFound();

        return Ok(new { message = "Stock updated successfully" });
    }
}
