namespace DoctorManagement.Service;

using DoctorManagement.Data;
using DoctorManagement.Models.User;
using DoctorManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDBContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDBContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ✅ REGISTER
    public async Task<AuthResponseDto> Register(RegisterRequest request)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name ?? "User",
            Email = request.Email,
            Password = hashedPassword,
            Role = request.Role ?? "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            Role = user.Role,
            Name = user.Name,
            Id = user.Id
        };
    }

    // ✅ LOGIN
    public async Task<AuthResponseDto?> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return null;

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            Role = user.Role,
            Name = user.Name,
            Id = user.Id
        };
    }

    // 🔐 JWT TOKEN
    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),     // ✅ IMPORTANT
            new Claim("Name", user.Name)               // ✅ custom claim
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}