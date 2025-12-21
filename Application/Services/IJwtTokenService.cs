using PlantHealthCheck.Domain.Entities;

namespace PlantHealthCheck.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
