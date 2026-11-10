using AutoFixture;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public class TestDataTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IInventoryService> _inventoryServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly CheckoutService _checkoutService;

    public TestDataTests()
    {
        _checkoutService = new CheckoutService(
            _orderRepositoryMock.Object,
            _inventoryServiceMock.Object,
            _notificationServiceMock.Object);
    }

    // Task 3.1
    [Fact]
    public async Task CheckoutAsync_WhenOrderCreatedWithAutoFixtureCustomization_ShouldSucceed()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new OrderCustomization());
        var order = fixture.Create<Order>();

        _inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var receipt = await _checkoutService.CheckoutAsync(order);

        // Assert
        receipt.Should().NotBeNull();
        receipt.OrderId.Should().Be(order.Id);
        receipt.CustomerName.Should().Be(order.CustomerName);
        receipt.CustomerEmail.Should().Be(order.CustomerEmail);
    }

    // Task 3.2
    [Fact]
    public async Task CheckoutAsync_WhenCustomerCreatedWithBogusFixedSeed_ShouldReturnReceiptWithMatchingCustomer()
    {
        // Arrange
        const int fixedSeed = 42;
        var customerFaker = new CustomerFaker(fixedSeed);
        var customer = customerFaker.Generate();

        var order = new Order
        {
            CustomerName = customer.FullName,
            CustomerEmail = customer.Email,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Course Item",
                    UnitPrice = 100m,
                    Quantity = 2
                }
            }
        };

        _inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var receipt = await _checkoutService.CheckoutAsync(order);

        // Assert
        receipt.CustomerName.Should().Be(customer.FullName);
        receipt.CustomerEmail.Should().Be(customer.Email);
    }
}

