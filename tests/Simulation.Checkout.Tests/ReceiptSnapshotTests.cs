using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests;

public class ReceiptSnapshotTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IInventoryService> _inventoryServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly CheckoutService _checkoutService;

    public ReceiptSnapshotTests()
    {
        _checkoutService = new CheckoutService(
            _orderRepositoryMock.Object,
            _inventoryServiceMock.Object,
            _notificationServiceMock.Object);
    }

    // Task 4.1
    [Fact]
    public async Task CheckoutAsync_WithExampleC_ShouldMatchSnapshot()
    {
        // Arrange
        var fixedOrderId = new Guid("11111111-1111-1111-1111-111111111111");
        var fixedProductId = new Guid("22222222-2222-2222-2222-222222222222");

        var order = new Order
        {
            Id = fixedOrderId,
            CustomerName = "Sarah Connor",
            CustomerEmail = "sarah.connor@example.com",
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = fixedProductId,
                    ProductName = "Premium Course Package",
                    UnitPrice = 500m,
                    Quantity = 3
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
        await Verify(receipt)
            .ScrubMember<CheckoutReceipt>(r => r.CreatedAt)
            .ScrubMember<CheckoutReceipt>(r => r.OrderId);
    }
}

