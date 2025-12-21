using MediatR;
using Microsoft.Extensions.Logging;
using PlantHealthCheck.Application.Commands;
using PlantHealthCheck.Application.DTOs;
using PlantHealthCheck.Domain.Entities;
using PlantHealthCheck.Domain.Repositories;

namespace PlantHealthCheck.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IUserRepository userRepository, ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "用户名不能为空"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "密码不能为空"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "邮箱不能为空"
            };
        }

        // Validate email format
        if (!IsValidEmail(request.Email))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "邮箱格式不正确"
            };
        }

        // Validate password strength (minimum 6 characters)
        if (request.Password.Length < 6)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "密码长度至少为6个字符"
            };
        }

        // Check if username or email already exists
        var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingByUsername != null)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "用户名已存在"
            };
        }

        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "邮箱已被注册"
            };
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = true
        };

        try
        {
            var createdUser = await _userRepository.AddAsync(user);

            _logger.LogInformation("User {Username} registered successfully with ID {UserId}", 
                createdUser.Username, createdUser.Id);

            return new RegisterResponse
            {
                Success = true,
                Message = "注册成功",
                User = new UserInfo
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", request.Username);
            return new RegisterResponse
            {
                Success = false,
                Message = "注册失败，请稍后再试"
            };
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
