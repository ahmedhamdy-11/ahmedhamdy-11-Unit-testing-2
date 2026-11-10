using Bogus;

namespace Simulation.Checkout.Tests.TestData;

public record CustomerTestData(string FullName, string Email);

public class CustomerFaker : Faker<CustomerTestData>
{
    public const int FixedSeed = 42;

    public CustomerFaker(int seed = FixedSeed)
    {
        UseSeed(seed);
        CustomInstantiator(f => new CustomerTestData(
            FullName: f.Name.FullName(),
            Email: f.Internet.Email()));
    }
}

