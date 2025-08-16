using Xunit;
using Moq;
using PedejaApp.Application.Services;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Application.Models;

namespace PedejaApp.Tests.Unit.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly SimpleProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _productService = new SimpleProductService(_mockProductRepository.Object, _mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.99m, IsActive = true },
                new Product { Id = 2, Name = "Product 2", Price = 20.99m, IsActive = false }
            };
            _mockProductRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "Product 1");
            Assert.Contains(result, p => p.Name == "Product 2");
        }

        [Fact]
        public async Task GetActiveProductsAsync_ReturnsOnlyActiveProducts()
        {
            // Arrange
            var activeProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Active Product", Price = 10.99m, IsActive = true }
            };
            _mockProductRepository.Setup(x => x.GetActiveProductsAsync()).ReturnsAsync(activeProducts);

            // Act
            var result = await _productService.GetActiveProductsAsync();

            // Assert
            Assert.Single(result);
            Assert.All(result, p => Assert.True(p.IsActive));
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test Product", Price = 15.99m };
            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(15.99m, result.Price);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProductAsync_WithValidProduct_ReturnsCreatedProduct()
        {
            // Arrange
            var product = new Product
            {
                Name = "New Product",
                Price = 25.99m,
                CategoryId = 1,
                IsActive = true
            };

            var createdProduct = new Product
            {
                Id = 1,
                Name = "New Product",
                Price = 25.99m,
                CategoryId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

            // Act
            var result = await _productService.CreateProductAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("New Product", result.Name);
            Assert.Equal(25.99m, result.Price);
            _mockProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidProduct_ReturnsUpdatedProduct()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Original Product",
                Price = 10.99m,
                CategoryId = 1,
                IsActive = true
            };

            var updatedProduct = new Product
            {
                Id = 1,
                Name = "Updated Product",
                Price = 15.99m,
                CategoryId = 1,
                IsActive = true
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingProduct);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(updatedProduct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product", result.Name);
            Assert.Equal(15.99m, result.Price);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidProduct_ThrowsArgumentException()
        {
            // Arrange
            var product = new Product { Id = 999, Name = "Non-existent Product" };
            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.UpdateProductAsync(product));
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_CallsRepositoryDelete()
        {
            // Arrange
            var productId = 1;
            _mockProductRepository.Setup(x => x.DeleteAsync(productId)).Returns(Task.CompletedTask);

            // Act
            await _productService.DeleteProductAsync(productId);

            // Assert
            _mockProductRepository.Verify(x => x.DeleteAsync(productId), Times.Once);
        }

        [Fact]
        public async Task ToggleProductStatusAsync_WithValidId_TogglesIsActiveStatus()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test Product", IsActive = true };
            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.ToggleProductStatusAsync(1);

            // Assert
            Assert.False(result.IsActive);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task ToggleProductStatusAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.ToggleProductStatusAsync(999));
        }

        [Fact]
        public async Task UpdateProductPriceAsync_WithValidIdAndPrice_UpdatesPrice()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test Product", Price = 10.99m };
            var newPrice = 15.99m;
            
            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductPriceAsync(1, newPrice);

            // Assert
            Assert.Equal(newPrice, result.Price);
            _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductPriceAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.UpdateProductPriceAsync(999, 15.99m));
        }

        [Fact]
        public async Task SearchProductsAsync_WithSearchTerm_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Pizza Margherita", Description = "Traditional pizza", IsActive = true },
                new Product { Id = 2, Name = "Pizza Pepperoni", Description = "Spicy pizza", IsActive = true },
                new Product { Id = 3, Name = "Pasta Carbonara", Description = "Creamy pasta", IsActive = true }
            };
            
            _mockProductRepository.Setup(x => x.GetActiveProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.SearchProductsAsync("pizza");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.True(p.Name.Contains("Pizza", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task SearchProductsAsync_WithEmptySearchTerm_ReturnsAllActiveProducts()
        {
            // Arrange
            var activeProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", IsActive = true },
                new Product { Id = 2, Name = "Product 2", IsActive = true }
            };
            
            _mockProductRepository.Setup(x => x.GetActiveProductsAsync()).ReturnsAsync(activeProducts);

            // Act
            var result = await _productService.SearchProductsAsync("");

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_WithValidCategoryId_ReturnsProductsInCategory()
        {
            // Arrange
            var categoryProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", CategoryId = 1 },
                new Product { Id = 2, Name = "Product 2", CategoryId = 1 }
            };
            
            _mockProductRepository.Setup(x => x.GetProductsByCategoryAsync(1)).ReturnsAsync(categoryProducts);

            // Act
            var result = await _productService.GetProductsByCategoryAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.CategoryId));
        }

        [Fact]
        public async Task CreateProductFromDtoAsync_WithValidRequest_ReturnsCreatedProduct()
        {
            // Arrange
            var request = new CreateProductRequest
            {
                Name = "DTO Product",
                Description = "Product from DTO",
                Price = 29.99m,
                CategoryId = 1,
                IsActive = true
            };

            var createdProduct = new Product
            {
                Id = 1,
                Name = "DTO Product",
                Description = "Product from DTO",
                Price = 29.99m,
                CategoryId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

            // Act
            var result = await _productService.CreateProductFromDtoAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Price, result.Price);
            Assert.Equal(request.CategoryId, result.CategoryId);
        }

        [Fact]
        public async Task UpdateProductFromDtoAsync_WithValidRequest_ReturnsUpdatedProduct()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Original Product",
                Price = 10.99m,
                CategoryId = 1
            };

            var request = new UpdateProductRequest
            {
                Name = "Updated Product",
                Price = 15.99m,
                CategoryId = 1,
                IsActive = true
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingProduct);
            _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductFromDtoAsync(1, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Price, result.Price);
        }

        [Fact]
        public async Task UpdateProductFromDtoAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            var request = new UpdateProductRequest { Name = "Product", Price = 10.99m };
            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _productService.UpdateProductFromDtoAsync(999, request));
        }
    }
}
