using RetailService.Core.DTOs;

namespace RetailService.Core.Services;

public interface IProductService
{
    Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchDto searchDto);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(Guid id);
    Task<bool> UpdateStockAsync(Guid productId, int quantity);
}
