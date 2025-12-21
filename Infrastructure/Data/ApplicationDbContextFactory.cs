using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlantHealthCheck.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a dummy connection string for migrations with a fixed MySQL version
        var connectionString = "Server=localhost;Database=plant_health_check;User=root;Password=yourpassword;";
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
