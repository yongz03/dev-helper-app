namespace DevHelper.Api.DTOs;

public sealed record CustomerResponse(int Id, string Name, string Email, DateTime CreatedAtUtc);
