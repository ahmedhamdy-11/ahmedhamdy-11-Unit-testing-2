
using AutoFixture;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;

namespace Simulation.Checkout.Tests
{
    public class CheckoutServiceTests
    {
        private readonly Mock<IOrderRepository> orderRepositoryMock = new();
        private readonly Mock<IInventoryService> inventoryServiceMock = new();
        private readonly Mock<INotificationService> notificationServiceMock = new();

        private readonly CheckoutService _checkoutService;

        public CheckoutServiceTests()
        {
            _checkoutService = new CheckoutService(
                orderRepository: orderRepositoryMock.Object,
                inventoryService: inventoryServiceMock.Object,
                notificationService: notificationServiceMock.Object);
        }

        // Task 1.1
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
        {
            // Arrange
            var fixture = new Fixture();

            var order = fixture.Build<Order>()
                .With(x => x.Items, new List<OrderItem>())
                .Create();

            // Act
            Func<Task> act = () => _checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<EmptyOrderException>();
        }

        // Task 1.2
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutAsync_WhenQuantityIsInvalid_ShouldThrowInvalidQuantityException(
            int invalidQuantity)
        {
            // Arrange
            var fixture = new Fixture();

            var orderItem = fixture.Build<OrderItem>()
                .With(x => x.Quantity, invalidQuantity)
                .With(x => x.ProductName, "TestProduct")
                .Create();

            var order = fixture.Build<Order>()
                .With(x => x.Items, new List<OrderItem> { orderItem })
                .Create();

            // Act
            Func<Task> act = () => _checkoutService.CheckoutAsync(order);

            // Assert
            var exceptionAssertion =
                await act.Should().ThrowAsync<InvalidQuantityException>();

            exceptionAssertion.Which.Message.Should().Contain("TestProduct");
        }

        // Task 1.3
        [Fact]
        public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
        {
            // Arrange
            var fixture = new Fixture();

            var orderItem = fixture.Build<OrderItem>()
                .With(x => x.UnitPrice, -0.01m)
                .With(x => x.ProductName, "TestProduct")
                .Create();

            var order = fixture.Build<Order>()
                .With(x => x.Items, new List<OrderItem> { orderItem })
                .Create();

            // Act
            Func<Task> act = () => _checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<InvalidPriceException>();
        }

        // Task 1.4
        [Fact]
        public async Task CheckoutAsync_WhenPriceIsZero_ShouldSucceed()
        {
            // Arrange
            var fixture = new Fixture();

            var orderItem = fixture.Build<OrderItem>()
                .With(x => x.UnitPrice, 0m)
                .With(x => x.Quantity, 1)
                .Create();

            var order = fixture.Build<Order>()
                .With(x => x.Items, new List<OrderItem> { orderItem })
                .Create();

            inventoryServiceMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = () => _checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // Task 1.5
        public static TheoryData<decimal, decimal> DiscountTestData => new()
        {
            { 999.99m, 0m },
            { 1000m, 100m },
            { 1500m, 100m }
        };

        [Theory]
        [MemberData(nameof(DiscountTestData))]
        public void CalculateDiscount_ShouldReturnCorrectDiscount(
            decimal subtotal,
            decimal expectedDiscount)
        {
            // Arrange & Act
            var result = CheckoutService.CalculateDiscount(subtotal);

            // Assert
            result.Should().Be(expectedDiscount);
        }

        // Task 1.6
        [Theory]
        [InlineData(150, 2, 400, 1, 700, 0, 700)]
        [InlineData(500, 3, 0, 0, 1500, 100, 1400)]
        public async Task CheckoutAsync_WithExampleOrders_ShouldCalculateCorrectTotals(
            decimal price1,
            int quantity1,
            decimal price2,
            int quantity2,
            decimal expectedSubtotal,
            decimal expectedDiscount,
            decimal expectedTotal)
        {
            // Arrange
            var fixture = new Fixture();

            var items = new List<OrderItem>
            {
                fixture.Build<OrderItem>()
                    .With(x => x.UnitPrice, price1)
                    .With(x => x.Quantity, quantity1)
                    .With(x => x.ProductName, "Product1")
                    .Create()
            };

            if (quantity2 > 0)
            {
                items.Add(
                    fixture.Build<OrderItem>()
                        .With(x => x.UnitPrice, price2)
                        .With(x => x.Quantity, quantity2)
                        .With(x => x.ProductName, "Product2")
                        .Create()
                );
            }

            var order = fixture.Build<Order>()
                .With(x => x.Items, items)
                .Create();

            inventoryServiceMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var receipt = await _checkoutService.CheckoutAsync(order);

            // Assert
            receipt.Subtotal.Should().Be(expectedSubtotal);
            receipt.Discount.Should().Be(expectedDiscount);
            receipt.Total.Should().Be(expectedTotal);
        }

        // Task 2.1
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsValid_ShouldSaveAndSendConfirmationOnce()
        {
            // Arrange
            orderRepositoryMock.Reset();
            inventoryServiceMock.Reset();
            notificationServiceMock.Reset();

            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "ahmed@test.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        UnitPrice = 1500m,
                        Quantity = 1
                    }
                }
            };

            inventoryServiceMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _checkoutService.CheckoutAsync(order);

            // Assert
            result.Should().NotBeNull();

            orderRepositoryMock.Verify(
                x => x.SaveAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            notificationServiceMock.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Task 2.2
        [Fact]
        public async Task CheckoutAsync_WhenItemIsOutOfStock_ShouldThrowAndNotSaveOrNotify()
        {
            // Arrange
            orderRepositoryMock.Reset();
            inventoryServiceMock.Reset();
            notificationServiceMock.Reset();

            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "ahmed@test.com",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Laptop",
                        UnitPrice = 1500m,
                        Quantity = 1
                    }
                }
            };

            inventoryServiceMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<InsufficientInventoryException>();

            orderRepositoryMock.Verify(
                x => x.SaveAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            notificationServiceMock.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // Task 2.3
        [Fact]
        public async Task CheckoutAsync_InvalidQuantity_ShouldNotCallAnyDependency()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Test Customer",
                CustomerEmail = "test@test.com",
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "TestProduct",
                UnitPrice = 100m,
                Quantity = 0
            }
        }
            };

            // Act
            Func<Task> act = async () => await _checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<InvalidQuantityException>();

            inventoryServiceMock.Verify(
                x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            orderRepositoryMock.Verify(
                x => x.SaveAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            notificationServiceMock.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
