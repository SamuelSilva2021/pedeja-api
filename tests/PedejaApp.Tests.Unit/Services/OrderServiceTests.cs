using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PedejaApp.Application.Services;
using PedejaApp.Application.Services.Interfaces;
using PedejaApp.Domain.Entities;
using PedejaApp.Domain.Interfaces;
using PedejaApp.Domain.DTOs;
using PedejaApp.Domain.Enums;
using PedejaApp.Application.Models;
using PedejaApp.Application.Common.Models;
using PedejaApp.Infrastructure.Repositories.Interfaces;
using PedejaApp.Application.Common.Responses;

namespace PedejaApp.Tests.Unit.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderMapper> _mockOrderMapper;
        private readonly Mock<IOrderValidationService> _mockValidationService;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderMapper = new Mock<IOrderMapper>();
            _mockValidationService = new Mock<IOrderValidationService>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            
            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockOrderMapper.Object,
                _mockValidationService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetOrdersAsync_ReturnsSuccessWithOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending },
                new Order { Id = 2, CustomerName = "Maria Santos", Status = OrderStatus.Confirmed }
            };
            var orderDtos = new List<OrderResponseDto>
            {
                new OrderResponseDto { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending },
                new OrderResponseDto { Id = 2, CustomerName = "Maria Santos", Status = OrderStatus.Confirmed }
            };

            _mockOrderRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(orders);
            _mockOrderMapper.Setup(x => x.ToResponseDtoList(orders)).Returns(orderDtos);

            // Act
            var result = await _orderService.GetOrdersAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count());
            Assert.Contains(result.Data, o => o.CustomerName == "João Silva");
            Assert.Contains(result.Data, o => o.CustomerName == "Maria Santos");
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithValidId_ReturnsSuccessWithOrder()
        {
            // Arrange
            var order = new Order { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending };
            var orderDto = new OrderResponseDto { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending };

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockOrderMapper.Setup(x => x.ToResponseDto(order)).Returns(orderDto);

            // Act
            var result = await _orderService.GetOrderByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("João Silva", result.Data.CustomerName);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.GetOrderByIdAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Pedido não encontrado.", result.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidRequest_ReturnsSuccessWithCreatedOrder()
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
            var order = new Order { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending };
            var orderDto = new OrderResponseDto { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockValidationService.Setup(x => x.ValidateCreateOrderAsync(createRequest)).ReturnsAsync(validationResult);
            _mockOrderMapper.Setup(x => x.ToEntity(createRequest)).Returns(order);
            _mockOrderRepository.Setup(x => x.AddAsync(order)).ReturnsAsync(order);
            _mockOrderMapper.Setup(x => x.ToResponseDto(order)).Returns(orderDto);

            // Act
            var result = await _orderService.CreateOrderAsync(createRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("João Silva", result.Data.CustomerName);
        }

        [Fact]
        public async Task CreateOrderAsync_WithInvalidRequest_ReturnsValidationError()
        {
            // Arrange
            var createRequest = new CreateOrderRequestDto
            {
                CustomerName = "", // Nome inválido
                Items = new List<CreateOrderItemRequestDto>()
            };
            var validationResult = ApiResponse<bool>.BadRequest("Nome do cliente é obrigatório.");

            _mockValidationService.Setup(x => x.ValidateCreateOrderAsync(createRequest)).ReturnsAsync(validationResult);

            // Act
            var result = await _orderService.CreateOrderAsync(createRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Nome do cliente é obrigatório.", result.Message);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithValidRequest_ReturnsSuccessWithUpdatedOrder()
        {
            // Arrange
            var updateRequest = new UpdateOrderRequestDto
            {
                CustomerName = "João Silva Atualizado",
                CustomerPhone = "11888888888"
            };
            var existingOrder = new Order { Id = 1, CustomerName = "João Silva", Status = OrderStatus.Pending };
            var updatedOrder = new Order { Id = 1, CustomerName = "João Silva Atualizado", Status = OrderStatus.Pending };
            var orderDto = new OrderResponseDto { Id = 1, CustomerName = "João Silva Atualizado", Status = OrderStatus.Pending };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingOrder);
            _mockValidationService.Setup(x => x.ValidateUpdateOrderAsync(1, updateRequest)).ReturnsAsync(validationResult);
            _mockOrderMapper.Setup(x => x.UpdateEntity(existingOrder, updateRequest));
            _mockOrderRepository.Setup(x => x.UpdateAsync(existingOrder)).ReturnsAsync(updatedOrder);
            _mockOrderMapper.Setup(x => x.ToResponseDto(updatedOrder)).Returns(orderDto);

            // Act
            var result = await _orderService.UpdateOrderAsync(1, updateRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("João Silva Atualizado", result.Data.CustomerName);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithNonExistentOrder_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new UpdateOrderRequestDto { CustomerName = "João Silva" };
            _mockOrderRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.UpdateOrderAsync(999, updateRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Pedido não encontrado.", result.Message);
        }

        [Fact]
        public async Task AcceptOrderAsync_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var updatedOrder = new Order { Id = 1, Status = OrderStatus.Confirmed };
            var orderDto = new OrderResponseDto { Id = 1, Status = OrderStatus.Confirmed };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockValidationService.Setup(x => x.ValidateAcceptOrderAsync(1)).ReturnsAsync(validationResult);
            _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(updatedOrder);
            _mockOrderMapper.Setup(x => x.ToResponseDto(updatedOrder)).Returns(orderDto);

            // Act
            var result = await _orderService.AcceptOrderAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(OrderStatus.Confirmed, result.Data.Status);
        }

        [Fact]
        public async Task RejectOrderAsync_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var updatedOrder = new Order { Id = 1, Status = OrderStatus.Cancelled };
            var orderDto = new OrderResponseDto { Id = 1, Status = OrderStatus.Cancelled };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockValidationService.Setup(x => x.ValidateRejectOrderAsync(1, "Produto indisponível")).ReturnsAsync(validationResult);
            _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(updatedOrder);
            _mockOrderMapper.Setup(x => x.ToResponseDto(updatedOrder)).Returns(orderDto);

            // Act
            var result = await _orderService.RejectOrderAsync(1, "Produto indisponível");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(OrderStatus.Cancelled, result.Data.Status);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WithValidTransition_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Confirmed };
            var updatedOrder = new Order { Id = 1, Status = OrderStatus.InPreparation };
            var orderDto = new OrderResponseDto { Id = 1, Status = OrderStatus.InPreparation };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockValidationService.Setup(x => x.ValidateStatusTransitionAsync(1, OrderStatus.InPreparation)).ReturnsAsync(validationResult);
            _mockOrderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(updatedOrder);
            _mockOrderMapper.Setup(x => x.ToResponseDto(updatedOrder)).Returns(orderDto);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(1, OrderStatus.InPreparation);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(OrderStatus.InPreparation, result.Data.Status);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithValidOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var validationResult = ApiResponse<bool>.Success(true);

            _mockOrderRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
            _mockValidationService.Setup(x => x.ValidateDeleteOrderAsync(1)).ReturnsAsync(validationResult);
            _mockOrderRepository.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.DeleteOrderAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Pedido excluído com sucesso.", result.Message);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithNonExistentOrder_ReturnsNotFound()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Order?)null);

            // Act
            var result = await _orderService.DeleteOrderAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Pedido não encontrado.", result.Message);
        }
    }
}