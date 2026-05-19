namespace ProductsApi.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token     { get; set; } = string.Empty;
    public string Username  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Role      { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role     { get; set; } = "User";
}

// ── Product ───────────────────────────────────────────────────────────────────

public class ProductDto
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public decimal  Price       { get; set; }
    public int      Stock       { get; set; }
    public string?  Category    { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class CreateProductDto
{
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public decimal  Price       { get; set; }
    public int      Stock       { get; set; }
    public string?  Category    { get; set; }
}

public class UpdateProductDto
{
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public decimal  Price       { get; set; }
    public int      Stock       { get; set; }
    public string?  Category    { get; set; }
}
