using Microsoft.EntityFrameworkCore;
using RetailService.Core.DTOs;
using RetailService.Core.Entities;
using RetailService.Core.Interfaces;
using RetailService.Core.Services;
using RetailService.Infrastructure.Data;

namespace RetailService.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly RetailDbContext _context;
    private readonly IProductRepository _productRepository;

    public ProductService(RetailDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }

    public async Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchDto searchDto)
    {
        // Using DbContext directly instead of repository - breaks abstraction
        var query = _context.Products.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            // SQL Injection risk? Should use parameterized query
            query = query.Where(p => p.Name.Contains(searchDto.SearchTerm) ||
                                    p.Description.Contains(searchDto.SearchTerm));
        }

        if (!string.IsNullOrEmpty(searchDto.Category))
        {
            query = query.Where(p => p.Category == searchDto.Category);
        }

        if (searchDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
        }

        if (searchDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
        }

        // Get total count - this could be slow on large datasets
        var totalCount = await query.CountAsync();

        // Apply pagination - no validation on PageSize
        var products = await query
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        // Mapping here instead of using AutoMapper or similar
        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Sku,
            p.StockQuantity,
            p.Category,
            p.IsActive
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

        return new ProductSearchResultDto(
            productDtos,
            totalCount,
            searchDto.Page,
            searchDto.PageSize,
            totalPages
        );
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Sku,
            product.StockQuantity,
            product.Category,
            product.IsActive
        );
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // No validation of SKU uniqueness before creation
        // Could cause database constraint violation

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Sku = dto.Sku,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category
        };

        var created = await _productRepository.CreateAsync(product);

        return new ProductDto(
            created.Id,
            created.Name,
            created.Description,
            created.Price,
            created.Sku,
            created.StockQuantity,
            created.Category,
            created.IsActive
        );
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        // Updating properties directly without validation
        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        var updated = await _productRepository.UpdateAsync(product);

        return new ProductDto(
            updated.Id,
            updated.Name,
            updated.Description,
            updated.Price,
            updated.Sku,
            updated.StockQuantity,
            updated.Category,
            updated.IsActive
        );
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        // Should check if product is part of any orders before deleting
        return await _productRepository.DeleteAsync(id);
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        // No concurrency control - race condition risk
        product.StockQuantity += quantity;

        // Could go negative!
        if (product.StockQuantity < 0)
        {
            throw new Exception("Insufficient stock");
        }

        await _productRepository.UpdateAsync(product);
        return true;
    }
}
