using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JwtBearerExample.API.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet(Name = "GetToken")]
    public IActionResult GetToken()
    {
        if (!double.TryParse(_configuration["ExpiresTokenInMinutes"], out var expiresTokenInMinutes))
        {
            expiresTokenInMinutes = 10;
        }

        var expires = DateTime.UtcNow.AddMinutes(expiresTokenInMinutes);

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_configuration["SecretKey"]);

        var subject = new ClaimsIdentity(new Claim[]
        {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Admin")
        });

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["Issuer"],
            Audience = _configuration["Audience"],
            Subject = subject,
            Expires = expires,
            SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var accessToken = tokenHandler.WriteToken(token);

        var result = new 
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expires
        };

        return Ok(result);
    }

    [HttpGet("NeedToken")]
    [Authorize]
    public IActionResult NeedToken()
    {
        return Ok();
    }
}

