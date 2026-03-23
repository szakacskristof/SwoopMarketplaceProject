using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SwoopMarketplaceProject.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ThormaBackendAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SwoopContext _sdbc;
    private readonly IConfiguration _cfg;

    public AuthController(UserManager<IdentityUser> users, IConfiguration cfg,SwoopContext sdbc)
    {
        _users = users;
        _sdbc=sdbc;
        _cfg = cfg;
    }

    public record RegisterRequest(string Email, string Password,string Phone);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = new IdentityUser { UserName = req.Email, Email = req.Email};
        var result = await _users.CreateAsync(user, req.Password);

        await _users.AddToRoleAsync(user, "User");
        
        if(result.Succeeded)
        {
            Guid guid = Guid.NewGuid();
            
            string randomusername = "User" + guid.ToString().Split("-")[0];

            User newUser = new User() {

                
                Email = user.Email,
                Username = randomusername,
                CreatedAt = DateTime.UtcNow,
                Bio = "",
                Phone = req.Phone,
                ProfileImageUrl = ""
                
            };

            await _sdbc.Users.AddAsync(newUser);
            await _sdbc.SaveChangesAsync();

        }
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }
         
      
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null) return Unauthorized();
        var ok = await _users.CheckPasswordAsync(user, req.Password);
        if (!ok) return Unauthorized();

        var roles = await _users.GetRolesAsync(user);
        var token = CreateJwt(user, roles);

        return Ok(new { token });
    }

    private string CreateJwt(IdentityUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? "")
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

   

}