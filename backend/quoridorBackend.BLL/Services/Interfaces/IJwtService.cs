using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.BLL.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}
