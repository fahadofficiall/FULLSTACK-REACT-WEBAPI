using System.Data;
using Microsoft.Data.SqlClient;
using ProductsApi.Interfaces;
using ProductsApi.Models;

namespace ProductsApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ── Map a SqlDataReader row → User ────────────────────────────────────────
    private static User MapUser(SqlDataReader reader) => new()
    {
        Id           = reader.GetInt32(reader.GetOrdinal("Id")),
        Username     = reader.GetString(reader.GetOrdinal("Username")),
        Email        = reader.GetString(reader.GetOrdinal("Email")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        Role         = reader.GetString(reader.GetOrdinal("Role")),
        CreatedAt    = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        IsActive     = reader.GetBoolean(reader.GetOrdinal("IsActive"))
    };

    // ── GetByUsername ─────────────────────────────────────────────────────────
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT Id, Username, Email, PasswordHash, Role, CreatedAt, IsActive
            FROM   Users
            WHERE  Username = @Username
              AND  IsActive  = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUser(reader) : null;
    }

    // ── GetById ───────────────────────────────────────────────────────────────
    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Username, Email, PasswordHash, Role, CreatedAt, IsActive
            FROM   Users
            WHERE  Id = @Id AND IsActive = 1";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUser(reader) : null;
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Username, @Email, @PasswordHash, @Role, GETUTCDATE(), 1)";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Username",     SqlDbType.NVarChar, 100).Value  = user.Username;
        cmd.Parameters.Add("@Email",        SqlDbType.NVarChar, 200).Value  = user.Email;
        cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value  = user.PasswordHash;
        cmd.Parameters.Add("@Role",         SqlDbType.NVarChar, 50).Value   = user.Role;

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // ── Existence checks ──────────────────────────────────────────────────────
    public async Task<bool> UsernameExistsAsync(string username)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
        var count = (int)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;
        var count = (int)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}
