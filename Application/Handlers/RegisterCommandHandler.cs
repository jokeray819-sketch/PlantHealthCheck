using MediatR;
using PlantHealthCheck.Application.Commands;
using PlantHealthCheck.Application.DTOs;
using PlantHealthCheck.Domain.Entities;
using PlantHealthCheck.Domain.Repositories;

namespace PlantHealthCheck.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
        if (await _userRepository.ExistsAsync(request.Username, request.Email))
        {
            // Check specifically which one exists
            var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
            var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);

            if (existingByUsername != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "用户名已存在"
                };
            }

            if (existingByEmail != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "邮箱已被注册"
                };
            }
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
        catch (Exception)
        {
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
