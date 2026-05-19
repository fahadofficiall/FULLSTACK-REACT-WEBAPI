using ProductsApi.Models;

namespace ProductsApi.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?>             GetByIdAsync(int id);
    Task<int>                  CreateAsync(Product product);
    Task<bool>                 UpdateAsync(Product product);
    Task<bool>                 DeleteAsync(int id);
    Task<bool>                 ExistsAsync(int id);
}
