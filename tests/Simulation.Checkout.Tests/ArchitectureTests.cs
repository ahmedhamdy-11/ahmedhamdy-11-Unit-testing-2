using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "Simulation.Checkout.Domain";
    private const string ApplicationNamespace = "Simulation.Checkout.Application";
    private const string InfrastructureNamespace = "Simulation.Checkout.Infrastructure";

    // Task 5.1
    [Fact]
    public void Domain_WhenCheckingDependencies_ShouldNotDependOnApplicationOrInfrastructure()
    {
        // Arrange
        var domainAssembly = typeof(Order).Assembly;

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Task 5.2
    [Fact]
    public void Application_WhenCheckingDependencies_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var applicationAssembly = typeof(CheckoutService).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Task 5.3
    [Fact]
    public void Interfaces_WhenCheckingNamingConventions_ShouldStartWithI()
    {
        // Arrange
        var applicationAssembly = typeof(CheckoutService).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    // Task 5.4
    [Fact]
    public void OrderRepositoryImplementations_WhenCheckingNamingConventions_ShouldEndWithRepository()
    {
        // Arrange
        var infrastructureAssembly = typeof(InMemoryOrderRepository).Assembly;

        // Act
        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}

