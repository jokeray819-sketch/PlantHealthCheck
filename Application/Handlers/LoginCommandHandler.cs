using MediatR;
using PlantHealthCheck.Application.Commands;
using PlantHealthCheck.Application.DTOs;
using PlantHealthCheck.Application.Services;
using PlantHealthCheck.Domain.Repositories;

namespace PlantHealthCheck.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "用户名和密码不能为空"
            };
        }

        // Find user
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "账户已被禁用"
            };
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "登录成功",
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };
    }
}
