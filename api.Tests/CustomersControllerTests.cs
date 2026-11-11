using DevHelper.Api.Controllers;
using DevHelper.Api.Data;
using DevHelper.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevHelper.Api.Tests;

public sealed class CustomersControllerTests
{
    private static AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    [Fact]
    public async Task CreateCustomer_ValidRequest_AddsCustomerAndReturnsCreated()
    {
        await using var context = CreateContext();
        var controller = new CustomersController(context);

        var result = await controller.CreateCustomer(new CreateCustomerRequest { Name = " Ada Lovelace ", Email = "ada@example.com" }, default);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<CustomerResponse>(created.Value);
        Assert.Equal("Ada Lovelace", response.Name);
        Assert.Equal("ada@example.com", response.Email);
        Assert.Single(context.Customers);
    }

    [Fact]
    public async Task CreateCustomer_InvalidName_ReturnsValidationProblem()
    {
        await using var context = CreateContext();
        var controller = new CustomersController(context);

        var result = await controller.CreateCustomer(new CreateCustomerRequest { Name = " ", Email = "ada@example.com" }, default);

        Assert.IsType<ObjectResult>(result.Result);
        Assert.Empty(context.Customers);
    }

    [Fact]
    public async Task GetCustomers_ReturnsCustomersOrderedByName()
    {
        await using var context = CreateContext();
        context.Customers.AddRange(
            new() { Name = "Zoe", Email = "zoe@example.com", CreatedAtUtc = DateTime.UtcNow },
            new() { Name = "Ada", Email = "ada@example.com", CreatedAtUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = new CustomersController(context);

        var result = await controller.GetCustomers(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var customers = Assert.IsType<List<CustomerResponse>>(ok.Value);
        Assert.Equal(new[] { "Ada", "Zoe" }, customers.Select(customer => customer.Name));
    }
}
