using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RetailService.API.Controllers;
using RetailService.Core.DTOs;
using RetailService.Core.Entities;
using RetailService.Core.Interfaces;
using FluentAssertions;
using Xunit;

namespace RetailService.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResultWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.99m, Sku = "SKU001", Category = "Test", IsActive = true },
            new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.99m, Sku = "SKU002", Category = "Test", IsActive = true }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
        returnedProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkResultWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 15.99m,
            Sku = "SKU001",
            Category = "Test",
            IsActive = true
        };
        _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Id.Should().Be(productId);
        returnedProduct.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto(
            "New Product",
            "Description",
            25.99m,
            "SKU003",
            100,
            "Electronics"
        );

        var createdProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            Sku = createDto.Sku,
            StockQuantity = createDto.StockQuantity,
            Category = createDto.Category,
            IsActive = true
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedProduct = createdResult.Value.Should().BeOfType<ProductDto>().Subject;
        returnedProduct.Name.Should().Be(createDto.Name);
        returnedProduct.Price.Should().Be(createDto.Price);
    }

    [Fact]
    public async Task Update_ExistingProduct_ReturnsOkResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Price = 10.99m,
            Sku = "SKU001",
            StockQuantity = 50,
            Category = "Test",
            IsActive = true
        };

        var updateDto = new UpdateProductDto(
            "New Name",
            "New Description",
            15.99m,
            75,
            true
        );

        _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(existingProduct);

        // Act
        var result = await _controller.Update(productId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(productId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(productId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
