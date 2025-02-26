using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CorrelationIdUpdateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Validate user credentials    
            var validUsername = _configuration["AuthSettings:Username"];
            var validPassword = _configuration["AuthSettings:Password"];

            if (model.Username != validUsername || model.Password != validPassword)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            // Generate JWT Token
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"];
            Console.WriteLine($"Key Length: {keyString.Length} characters"); // Ensure this outputs 32 or more
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "Admin") // Add roles for authorization
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: credentials
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }

    // Login Model
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
