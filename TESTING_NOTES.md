# Testing Notes

### 1. Total tests, passing, failing
- **Total tests**: 20 (16 test methods across all test classes, expanding to 20 test executions with theory data)
- **Passing**: 20
- **Failing**: 0

### 2. Line coverage % (from coverage-report/Summary.txt)
- **Overall Line Coverage**: 90.5%
- **Simulation.Checkout.Application Line Coverage**: 95.8%
- **Simulation.Checkout.Domain Line Coverage**: 100%

### 3. One sentence each: where you used Moq, AutoFixture and Bogus
- **Moq**: Moq was used across the test suite to mock the checkout external dependencies (`IOrderRepository`, `IInventoryService`, and `INotificationService`) and to verify expected call invocations and counts (`Times.Once`, `Times.Never`).
- **AutoFixture**: AutoFixture was used in `CheckoutServiceTests` and `TestDataTests` with custom `OrderCustomization` to generate valid `Order` and `OrderItem` objects with positive quantities and prices.
- **Bogus**: Bogus was used in `CustomerFaker` with a fixed seed (`seed = 42`) to generate realistic customer names and email addresses and verify they are correctly preserved on the checkout receipt.

### 4. Look at Examples B and C. Is it fair that a customer who pays 999.99 pays more than one who buys for 1,000? (One sentence — do not change the code.)
No, it is unfair because the rigid threshold creates a pricing cliff where an order totaling 999.99 receives no discount and costs 999.99, whereas adding one cent to reach 1,000 triggers a 10% discount and drops the total cost to 900.

### 5. Any bug you think you found (or "none")
none

