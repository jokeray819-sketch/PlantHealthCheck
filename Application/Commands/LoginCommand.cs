using MediatR;
using PlantHealthCheck.Application.DTOs;

namespace PlantHealthCheck.Application.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
