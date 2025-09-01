using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Enums;
using PedejaApp.Application.Common.Models;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Application.Common.Responses;

namespace PedejaApp.Tests.Unit.Services
{
    public class OrderValidationServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<OrderValidationService>> _mockLogger;
        private readonly OrderValidationService _validationService;

        public OrderValidationServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<OrderValidationService>>();
            
            _validationService = new OrderValidationService(
                _mockOrderRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ValidateCreateOrderAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "João Silva",
                CustomerPhone = "11999999999",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2, UnitPrice = 15.99m }
                }
            };
            var product = new Product { Id = 1, Name = "Produto Teste", Price = 15.99m, IsActive = true };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _validationService.ValidateCreateOrderAsync(createRequest);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateCreateOrderAsync_WithEmptyCustomerName_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "",
                CustomerPhone = "11999999999",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2, UnitPrice = 15.99m }
                }
            };

            // Act
            var result = await _validationService.ValidateCreateOrderAsync(createRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Nome do cliente é obrigatório.", result.Message);
        }

        [Fact]
        public async Task ValidateCreateOrderAsync_WithEmptyItems_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "João Silva",
                CustomerPhone = "11999999999",
                Items = new List<CreateOrderItemRequestDto>()
            };

            // Act
            var result = await _validationService.ValidateCreateOrderAsync(createRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("O pedido deve conter pelo menos um item.", result.Message);
        }

        [Fact]
        public async Task ValidateCreateOrderAsync_WithInvalidProduct_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "João Silva",
                CustomerPhone = "11999999999",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 999, Quantity = 2, UnitPrice = 15.99m }
                }
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            var result = await _validationService.ValidateCreateOrderAsync(createRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Produto com ID 999 não encontrado.", result.Message);
        }

        [Fact]
        public async Task ValidateCreateOrderAsync_WithInactiveProduct_ReturnsBadRequest()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "João Silva",
                CustomerPhone = "11999999999",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2, UnitPrice = 15.99m }
                }
            };
            var product = new Product { Id = 1, Name = "Produto Teste", Price = 15.99m, IsActive = false };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _validationService.ValidateCreateOrderAsync(createRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Produto 'Produto Teste' não está disponível.", result.Message);
        }

        [Fact]
        public async Task ValidateUpdateOrderAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new UpdateOrderRequestDto
            {
                CustomerName = "João Silva Atualizado",
                CustomerPhone = "11888888888"
            };
            var order = new Order { Id = 1, Status = OrderStatus.Pending };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateUpdateOrderAsync(1, updateRequest);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateUpdateOrderAsync_WithNonEditableStatus_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UpdateOrderRequestDto
            {
                CustomerName = "João Silva Atualizado"
            };
            var order = new Order { Id = 1, Status = OrderStatus.Completed };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateUpdateOrderAsync(1, updateRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Não é possível editar um pedido com status 'Completed'.", result.Message);
        }

        [Fact]
        public async Task ValidateAcceptOrderAsync_WithPendingOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new AcceptOrderRequestDto();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateAcceptOrderAsync(1, request);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateAcceptOrderAsync_WithNonPendingOrder_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Confirmed };
            var request = new AcceptOrderRequestDto();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateAcceptOrderAsync(1, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Apenas pedidos pendentes podem ser aceitos.", result.Message);
        }

        [Fact]
        public async Task ValidateRejectOrderAsync_WithPendingOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new RejectOrderRequestDto { Reason = "Produto indisponível" };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateRejectOrderAsync(1, request);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateRejectOrderAsync_WithEmptyReason_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new RejectOrderRequestDto { Reason = "" };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateRejectOrderAsync(1, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Motivo da rejeição é obrigatório.", result.Message);
        }

        [Fact]
        public async Task ValidateStatusChangeAsync_WithValidTransition_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Confirmed };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateStatusChangeAsync(1, OrderStatus.InPreparation);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateStatusChangeAsync_WithInvalidTransition_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateStatusChangeAsync(1, OrderStatus.Completed);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Transição de status inválida: de 'Pending' para 'Completed'.", result.Message);
        }

        [Fact]
        public async Task ValidateDeleteOrderAsync_WithDeletableOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateDeleteOrderAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateDeleteOrderAsync_WithNonDeletableOrder_ReturnsBadRequest()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Completed };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);

            // Act
            var result = await _validationService.ValidateDeleteOrderAsync(1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Não é possível excluir um pedido com status 'Completed'.", result.Message);
        }

        [Fact]
        public async Task ValidateOrderItemsAsync_WithValidItems_ReturnsSuccess()
        {
            // Arrange
            var product = new Product { Id = 1, IsActive = true, Stock = 10 };
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 5 } };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _validationService.ValidateOrderItemsAsync(items);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateOrderItemsAsync_WithInactiveProduct_ReturnsBadRequest()
        {
            // Arrange
            var product = new Product { Id = 1, IsActive = false, Stock = 10 };
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 5 } };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _validationService.ValidateOrderItemsAsync(items);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Produto com ID 1 não está ativo.", result.Message);
        }

        [Fact]
        public async Task ValidateOrderItemsAsync_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            var product = new Product { Id = 1, IsActive = true, Stock = 3 };
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 5 } };

            _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _validationService.ValidateOrderItemsAsync(items);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Estoque insuficiente para o produto com ID 1. Disponível: 3, Solicitado: 5.", result.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ValidateItemAsync_WithInvalidQuantity_ReturnsBadRequest(int quantity)
        {
            // Arrange
            var item = new CreateOrderItemRequestDto
            {
                ProductId = 1,
                Quantity = quantity,
                UnitPrice = 15.99m
            };

            // Act
            var result = await _validationService.ValidateItemAsync(item);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Quantidade deve ser maior que zero.", result.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ValidateItemAsync_WithInvalidPrice_ReturnsBadRequest(decimal price)
        {
            // Arrange
            var item = new CreateOrderItemRequestDto
            {
                ProductId = 1,
                Quantity = 2,
                UnitPrice = price
            };

            // Act
            var result = await _validationService.ValidateItemAsync(item);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Preço unitário deve ser maior que zero.", result.Message);
        }

        [Fact]
        public void ValidateCustomerInfo_WithValidInfo_ReturnsSuccess()
        {
            // Arrange
            var customerName = "João Silva";
            var customerPhone = "11999999999";

            // Act
            var result = _validationService.ValidateCustomerInfo(customerName, customerPhone);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ValidateCustomerInfo_WithInvalidPhone_ReturnsBadRequest()
        {
            // Arrange
            var customerName = "João Silva";
            var customerPhone = "123"; // Telefone inválido

            // Act
            var result = _validationService.ValidateCustomerInfo(customerName, customerPhone);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Formato de telefone inválido.", result.Message);
        }
    }
}