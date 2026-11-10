using Bogus;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public class OrderFaker : Faker<Order>
{
    public const int FixedSeed = 42;

    public OrderFaker(int seed = FixedSeed)
    {
        UseSeed(seed);
        RuleFor(o => o.Id, f => f.Random.Guid());
        RuleFor(o => o.CustomerName, f => f.Name.FullName());
        RuleFor(o => o.CustomerEmail, f => f.Internet.Email());
        RuleFor(o => o.Items, f => new List<OrderItem>
        {
            new()
            {
                ProductId = f.Random.Guid(),
                ProductName = f.Commerce.ProductName(),
                UnitPrice = decimal.Round(f.Random.Decimal(10m, 500m), 2),
                Quantity = f.Random.Int(1, 5)
            }
        });
    }
}

