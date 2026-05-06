namespace RetailService.Core.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    string Category,
    bool IsActive
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    string Category
);

public record UpdateProductDto(
    string? Name,
    string? Description,
    decimal? Price,
    int? StockQuantity,
    bool? IsActive
);
