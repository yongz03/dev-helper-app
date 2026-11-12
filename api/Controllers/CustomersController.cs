using DevHelper.Api.Data;
using DevHelper.Api.DTOs;
using DevHelper.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHelper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetCustomers(CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers.AsNoTracking()
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerResponse(customer.Id, customer.Name, customer.Email, customer.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim();
        if (name.Length < 2 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            ModelState.AddModelError(nameof(request), "Enter a valid name and email address.");
            return ValidationProblem(ModelState);
        }

        if (await dbContext.Customers.AnyAsync(customer => customer.Email == email, cancellationToken))
        {
            ModelState.AddModelError(nameof(request.Email), "A customer with this email already exists.");
            return ValidationProblem(ModelState);
        }

        var customer = new Customer { Name = name, Email = email, CreatedAtUtc = DateTime.UtcNow };
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CustomerResponse(customer.Id, customer.Name, customer.Email, customer.CreatedAtUtc);
        return CreatedAtAction(nameof(GetCustomers), response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(
        int id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FindAsync([id], cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        var email = request.Email.Trim();
        if (name.Length < 2 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            ModelState.AddModelError(nameof(request), "Enter a valid name and email address.");
            return ValidationProblem(ModelState);
        }

        if (await dbContext.Customers.AnyAsync(
                existingCustomer => existingCustomer.Id != id && existingCustomer.Email == email,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(request.Email), "A customer with this email already exists.");
            return ValidationProblem(ModelState);
        }

        customer.Name = name;
        customer.Email = email;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CustomerResponse(customer.Id, customer.Name, customer.Email, customer.CreatedAtUtc));
    }
}
