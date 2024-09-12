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

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWork unitOfWork, IConfiguration config) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _config = config;

        //[Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {
            var users = await _unitOfWork.Users.GetAsync(orderBy:q=> q.OrderBy(u=>u.FirstName));
            var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();
            return Ok(new
            {
                Users = userDtos,
                Status = StatusCodes.Status200OK
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
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

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
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
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

            var userDto = UsersMapper.ToUserDto(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.PhoneNumber != command.PhoneNumber)
            {
                var phoneExists = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == command.PhoneNumber);
                if (phoneExists != null)
                {
                    Dictionary<string,string> errors = new Dictionary<string, string>();
                    errors.Add("PHONE_NUMBER", "Phone number exists in the database");
                    return BadRequest(new
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Error in validating data",
                        Errors = errors
                    });
                }
            }

            if (user.Email != command.Email)
            {
                var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == command.Email);
                if (emailExists.Any())
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    errors.Add("EMAIL_ADDRESS", "Email address exists in the database");
                    return BadRequest(new
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Error in validating data",
                        Errors = errors
                    });
                }
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

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
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

        [HttpGet]
        [Route("/test")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetFilterOrderBy()
        {
            var users = await _unitOfWork.Users.FindAsync(
                u => u.Lastname == "yaghi",
                o => o.OrderBy(u => u.FirstName)
            );

            return Ok(users);
        }

        [HttpGet("throw")]
        public IActionResult ThrowException()
        {
            throw new Exception("This is a test exception to check exception handling middleware.");
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin login)
        {
            // Simple validation for demo purposes (replace with real validation logic)
            if (login.Email == "yaghi@gmail.com" && login.FirstName == "password")
            {
                var token = GenerateJwtToken(login);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(UserLogin login)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, login.Email),
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

        [Authorize]
        [HttpGet("userinfo")]
        public IActionResult GetTokenInfo()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return Ok(new { Claims = claims });
        }
    }

    public class UserLogin
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
    }


}





//TODO: Override the Http response handler
//TODO: Edit the Get to let it sort results based on columns