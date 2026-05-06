using Microsoft.AspNetCore.Mvc;
using RetailService.Core.DTOs;
using RetailService.Core.Entities;
using RetailService.Core.Interfaces;

namespace RetailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] string? category = null)
    {
        _logger.LogInformation("Fetching products. Category filter: {Category}", category ?? "None");

        var products = string.IsNullOrEmpty(category)
            ? await _productRepository.GetAllAsync()
            : await _productRepository.GetByCategoryAsync(category);

        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Sku,
            p.StockQuantity,
            p.Category,
            p.IsActive
        ));

        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound();
        }

        var productDto = new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Sku,
            product.StockQuantity,
            product.Category,
            product.IsActive
        );

        return Ok(productDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createDto.Name);

        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Sku = createDto.Sku,
            StockQuantity = createDto.StockQuantity,
            Category = createDto.Category
        };

        var createdProduct = await _productRepository.CreateAsync(product);

        var productDto = new ProductDto(
            createdProduct.Id,
            createdProduct.Name,
            createdProduct.Description,
            createdProduct.Price,
            createdProduct.Sku,
            createdProduct.StockQuantity,
            createdProduct.Category,
            createdProduct.IsActive
        );

        return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto updateDto)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            return NotFound();
        }

        if (updateDto.Name != null) product.Name = updateDto.Name;
        if (updateDto.Description != null) product.Description = updateDto.Description;
        if (updateDto.Price.HasValue) product.Price = updateDto.Price.Value;
        if (updateDto.StockQuantity.HasValue) product.StockQuantity = updateDto.StockQuantity.Value;
        if (updateDto.IsActive.HasValue) product.IsActive = updateDto.IsActive.Value;

        var updatedProduct = await _productRepository.UpdateAsync(product);

        var productDto = new ProductDto(
            updatedProduct.Id,
            updatedProduct.Name,
            updatedProduct.Description,
            updatedProduct.Price,
            updatedProduct.Sku,
            updatedProduct.StockQuantity,
            updatedProduct.Category,
            updatedProduct.IsActive
        );

        return Ok(productDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);

        var result = await _productRepository.DeleteAsync(id);
        if (!result)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return NotFound();
        }

        return NoContent();
    }
}
