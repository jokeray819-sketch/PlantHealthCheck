using MediatR;
using PlantHealthCheck.Application.DTOs;

namespace PlantHealthCheck.Application.Commands;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
