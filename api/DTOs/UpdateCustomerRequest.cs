using System.ComponentModel.DataAnnotations;

namespace DevHelper.Api.DTOs;

public sealed class UpdateCustomerRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;
}
