using Core.IUnitOfWork;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_First_Project.Controllers
{
    public class ActionsController(IUnitOfWork unitOfWork, CatFactService catFactService ,IConfiguration config) : Controller
    {
        private IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _config = config;
        private readonly CatFactService _catFactService = catFactService;




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var tenant = await _unitOfWork.Tenants.FindSingleAsync(t => t.Name == login.Name);
            if (tenant != null)
            {
                var token = GenerateJwtToken(tenant);
                return Ok(new { token });
            }

            return BadRequest(new
            {
                Message = "Username or Password is incorrect"
            });
        }

        private string GenerateJwtToken(Tenant tenant)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, tenant.Name),
            new Claim("tenantId", tenant.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomCatFact()
        {
            var fact = await _catFactService.GetRandomCatFactAsync();
            return Ok(new { Fact = fact });
        }
    }
    public class Login
    {
        public string Name { get; set; } = string.Empty;
    }
}