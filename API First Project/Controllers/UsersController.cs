using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Core.IUnitOfWork;
using API_First_Project.Commands;
using API_First_Project.Dtos;
using API_First_Project.Mappers;
using API_First_Project.ErrorResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWork unitOfWork, IConfiguration config) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _config = config;

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {
            var tenantId = GetTenantId();
            if (tenantId > 0)
            {
                var users = await _unitOfWork.Users.FindAsync(
                filter: f => f.TenantId == tenantId,
                orderBy: q => q.OrderBy(u => u.FirstName));
                var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();
                return Ok(new
                {
                    Users = userDtos,
                    Status = StatusCodes.Status200OK
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var tenantId = GetTenantId();
            if (tenantId > 0)
            {
                var user = await _unitOfWork.Users.FindSingleAsync(filter: f => f.TenantId == tenantId && f.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    User = UsersMapper.ToUserDto(user),
                    Status = StatusCodes.Status200OK
                });
            }
            return Unauthorized();
        }
        
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {
            var tenantId = GetTenantId();
            if (tenantId > 0)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = new User
                {
                    Email = command.Email,
                    FirstName = command.FirstName,
                    Lastname = command.Lastname,
                    PhoneNumber = command.PhoneNumber,
                    Gender = command.Gender,
                    TenantId = tenantId
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveAsync();

                var userDto = UsersMapper.ToUserDto(user);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            var tenantId = GetTenantId();
            if (tenantId > 0)
            {
                var user = await _unitOfWork.Users.FindSingleAsync(filter: f => f.TenantId == tenantId && f.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = command.FirstName;
                user.Lastname = command.Lastname;
                user.PhoneNumber = command.PhoneNumber;
                user.Gender = command.Gender;
                user.Email = command.Email;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    User = UsersMapper.ToUserDto(user),
                    Status = StatusCodes.Status200OK
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var tenantId = GetTenantId();
            if (tenantId > 0)
            {
                var user = await _unitOfWork.Users.FindSingleAsync(filter: f => f.TenantId == tenantId && f.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                _unitOfWork.Users.Delete(user);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    Status = StatusCodes.Status200OK,
                    Message = "User deleted"
                });
            }
            return Unauthorized();
        }

        //[HttpGet]
        //[Route("/test")]
        //public async Task<ActionResult<IEnumerable<UserDto>>> GetFilterOrderBy()
        //{
        //    var users = await _unitOfWork.Users.FindAsync(
        //        u => u.Lastname == "yaghi",
        //        o => o.OrderBy(u => u.FirstName)
        //    );

        //    return Ok(users);
        //}

        //[HttpGet("throw")]
        //public IActionResult ThrowException()
        //{
        //    throw new Exception("This is a test exception to check exception handling middleware.");
        //}


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var tenant = await _unitOfWork.Tenants.FindSingleAsync(t => t.Name == login.Name);
            if (tenant != null)
            {
                var token = GenerateJwtToken(tenant);
                return Ok(new { token });
            }

            return Unauthorized();
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
        private int GetTenantId()
        {
            var user = HttpContext.User;

            var tenantIdClaim = user.Claims.FirstOrDefault(c => c.Type == "tenantId") ?? throw new UnauthorizedAccessException("TenantId claim not found in token.");
            if (int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }
            else
            {
                throw new InvalidOperationException("Invalid tenant ID format.");
            }
        }
    }


    public class Login
    {
        public string Name { get; set; }
    }

}




// Alert: {
//"error": "An unexpected error occurred. Please try again later.",
//    "details": "A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913."
//}