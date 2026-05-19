using System.Data;
using Microsoft.Data.SqlClient;
using ProductsApi.Interfaces;
using ProductsApi.Models;

namespace ProductsApi.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ── Map a SqlDataReader row → Product ─────────────────────────────────────
    private static Product MapProduct(SqlDataReader reader) => new()
    {
        Id          = reader.GetInt32(reader.GetOrdinal("Id")),
        Name        = reader.GetString(reader.GetOrdinal("Name")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description")),
        Price       = reader.GetDecimal(reader.GetOrdinal("Price")),
        Stock       = reader.GetInt32(reader.GetOrdinal("Stock")),
        Category    = reader.IsDBNull(reader.GetOrdinal("Category"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Category")),
        CreatedAt   = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        UpdatedAt   = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
        IsActive    = reader.GetBoolean(reader.GetOrdinal("IsActive"))
    };

    // ── GetAll ────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Name, Description, Price, Stock, Category, CreatedAt, UpdatedAt, IsActive
            FROM   Products
            WHERE  IsActive = 1
            ORDER  BY Name ASC";

        var products = new List<Product>();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            products.Add(MapProduct(reader));

        return products;
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Description, Price, Stock, Category, CreatedAt, UpdatedAt, IsActive
            FROM   Products
            WHERE  Id = @Id AND IsActive = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProduct(reader) : null;
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<int> CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, Category, CreatedAt, UpdatedAt, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @Price, @Stock, @Category, GETUTCDATE(), GETUTCDATE(), 1)";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Name",        SqlDbType.NVarChar, 200).Value  = product.Name;
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value =
            (object?)product.Description ?? DBNull.Value;
        cmd.Parameters.Add("@Price",    SqlDbType.Decimal).Value   = product.Price;
        cmd.Parameters["@Price"].Precision = 18;
        cmd.Parameters["@Price"].Scale     = 2;
        cmd.Parameters.Add("@Stock",    SqlDbType.Int).Value       = product.Stock;
        cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value =
            (object?)product.Category ?? DBNull.Value;

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<bool> UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE Products
            SET    Name        = @Name,
                   Description = @Description,
                   Price       = @Price,
                   Stock       = @Stock,
                   Category    = @Category,
                   UpdatedAt   = GETUTCDATE()
            WHERE  Id = @Id AND IsActive = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id",          SqlDbType.Int).Value            = product.Id;
        cmd.Parameters.Add("@Name",        SqlDbType.NVarChar, 200).Value  = product.Name;
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value =
            (object?)product.Description ?? DBNull.Value;
        cmd.Parameters.Add("@Price",    SqlDbType.Decimal).Value   = product.Price;
        cmd.Parameters["@Price"].Precision = 18;
        cmd.Parameters["@Price"].Scale     = 2;
        cmd.Parameters.Add("@Stock",    SqlDbType.Int).Value       = product.Stock;
        cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value =
            (object?)product.Category ?? DBNull.Value;

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ── Delete (soft delete) ──────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE Products
            SET    IsActive = 0, UpdatedAt = GETUTCDATE()
            WHERE  Id = @Id AND IsActive = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ── Exists ────────────────────────────────────────────────────────────────
    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Products WHERE Id = @Id AND IsActive = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        var count = (int)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}
