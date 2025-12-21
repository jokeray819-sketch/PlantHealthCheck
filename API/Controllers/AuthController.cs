using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlantHealthCheck.Application.Commands;
using PlantHealthCheck.Application.DTOs;

namespace PlantHealthCheck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>注册结果</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                Phone = request.Phone
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("User {Username} registered successfully", request.Username);
                return Ok(result);
            }

            _logger.LogWarning("Failed registration attempt for user {Username}: {Message}", 
                request.Username, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new RegisterResponse
                {
                    Success = false,
                    Message = "注册过程中发生错误"
                });
        }
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>登录结果</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("User {Username} logged in successfully", request.Username);
                return Ok(result);
            }

            _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new LoginResponse
                {
                    Success = false,
                    Message = "登录过程中发生错误"
                });
        }
    }
}
