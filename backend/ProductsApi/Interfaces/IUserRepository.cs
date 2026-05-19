using ProductsApi.Models;

namespace ProductsApi.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<int>   CreateAsync(User user);
    Task<bool>  UsernameExistsAsync(string username);
    Task<bool>  EmailExistsAsync(string email);
}
