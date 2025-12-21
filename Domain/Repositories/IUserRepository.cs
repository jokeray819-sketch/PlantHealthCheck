using PlantHealthCheck.Domain.Entities;

namespace PlantHealthCheck.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string username, string email);
}
