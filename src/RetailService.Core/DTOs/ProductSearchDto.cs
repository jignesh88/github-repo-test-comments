namespace RetailService.Core.DTOs;

public record ProductSearchDto(
    string? SearchTerm,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 10
);

public record ProductSearchResultDto(
    List<ProductDto> Products,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
