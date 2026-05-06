using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RetailService.API.Controllers;
using RetailService.Core.DTOs;
using RetailService.Core.Services;
using FluentAssertions;
using Xunit;

namespace RetailService.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Search_ReturnsOkResultWithProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto(null, null, null, null, 1, 10);
        var products = new List<ProductDto>
        {
            new ProductDto(Guid.NewGuid(), "Product 1", "Desc", 10.99m, "SKU001", 5, "Test", true),
            new ProductDto(Guid.NewGuid(), "Product 2", "Desc", 20.99m, "SKU002", 10, "Test", true)
        };
        var searchResult = new ProductSearchResultDto(products, 2, 1, 10, 1);
        _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<ProductSearchDto>())).ReturnsAsync(searchResult);

        // Act
        var result = await _controller.Search(searchDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ProductSearchResultDto>().Subject;
        returnedResult.Products.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkResultWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto(productId, "Test Product", "Desc", 15.99m, "SKU001", 5, "Test", true);
        _mockProductService.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductService.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
