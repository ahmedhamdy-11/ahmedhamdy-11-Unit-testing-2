using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public class OrderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<OrderItem>(composer => composer
            .With(x => x.Quantity, () => Random.Shared.Next(1, 10))
            .With(x => x.UnitPrice, () => decimal.Round((decimal)Random.Shared.Next(100, 5000) / 10m, 2)));

        fixture.Customize<Order>(composer => composer
            .With(x => x.Items, () => fixture.CreateMany<OrderItem>(2).ToList()));
    }
}

