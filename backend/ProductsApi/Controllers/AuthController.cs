using Microsoft.AspNetCore.Mvc;
using ProductsApi.DTOs;
using ProductsApi.Interfaces;
using ProductsApi.Models;
using ProductsApi.Services;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly JwtService      _jwtService;

    public AuthController(IUserRepository userRepo, JwtService jwtService)
    {
        _userRepo   = userRepo;
        _jwtService = jwtService;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Username and password are required." });

        var user = await _userRepo.GetByUsernameAsync(dto.Username.Trim());
        if (user is null)
            return Unauthorized(new { message = "Invalid username or password." });

        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!valid)
            return Unauthorized(new { message = "Invalid username or password." });

        var (token, expires) = _jwtService.GenerateToken(user);

        return Ok(new LoginResponseDto
        {
            Token    = token,
            Username = user.Username,
            Email    = user.Email,
            Role     = user.Role,
            Expires  = expires
        });
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Email)    ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "All fields are required." });

        if (await _userRepo.UsernameExistsAsync(dto.Username.Trim()))
            return Conflict(new { message = "Username already taken." });

        if (await _userRepo.EmailExistsAsync(dto.Email.Trim()))
            return Conflict(new { message = "Email already in use." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11);

        var user = new User
        {
            Username     = dto.Username.Trim(),
            Email        = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = "User"  // default role; only admins can set Admin
        };

        var newId = await _userRepo.CreateAsync(user);

        return CreatedAtAction(nameof(Login), new { message = "User created.", userId = newId });
    }

    // POST api/auth/seed  (DEV ONLY — remove in production)
    // Creates seeded admin/user accounts with hashed passwords
    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        var seeds = new[]
        {
            new { Username = "admin", Email = "admin@productsdb.com", Password = "admin123", Role = "Admin" },
            new { Username = "john",  Email = "john@productsdb.com",  Password = "user123",  Role = "User"  }
        };

        var results = new List<object>();

        foreach (var s in seeds)
        {
            if (await _userRepo.UsernameExistsAsync(s.Username))
            {
                results.Add(new { s.Username, status = "already exists" });
                continue;
            }

            var user = new User
            {
                Username     = s.Username,
                Email        = s.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(s.Password, workFactor: 11),
                Role         = s.Role
            };

            var id = await _userRepo.CreateAsync(user);
            results.Add(new { s.Username, status = "created", id });
        }

        return Ok(results);
    }
}
