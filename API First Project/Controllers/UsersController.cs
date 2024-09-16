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
using Infrastructure.Services.TenantIdGetter;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWork unitOfWork, ITenantService tenantService) : CustomControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITenantService _tenantService = tenantService;

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {
            var users = await _unitOfWork.Users.GetAsync();
            var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();

            return Ok(new
            { 
                Users = userDtos
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(f => f.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                User = UsersMapper.ToUserDto(user)
            });
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {
            
            var phoneExists = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == command.PhoneNumber);
            if (phoneExists != null)
            {
                return BadRequest(new
                {
                    status = "Phone_Already_Exists",
                    message = "Phone number must be Unique"
                });
            }

            var emailExists = await _unitOfWork.Users.FindSingleAsync(u => u.Email == command.Email);
            if (emailExists != null)
            {
                return BadRequest(new
                {
                    status = "Email_Aleady_Exists",
                    message = "Email address must be Unique"
                });
            }

            var user = new User
            {
                Email = command.Email,
                FirstName = command.FirstName,
                Lastname = command.Lastname,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

            var userDto = UsersMapper.ToUserDto(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(filter: f => f.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.PhoneNumber != command.PhoneNumber)
            {
                var phoneExists = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == command.PhoneNumber);
                if (phoneExists != null)
                {
                    return BadRequest(new
                    {
                        status = "Phone_Already_Exists",
                        message = "Phone number must be Unique"
                    });
                }
            }

            if (user.Email != command.Email)
            {
                var emailExists = await _unitOfWork.Users.FindSingleAsync(u => u.Email == command.Email);
                if (emailExists != null)
                {
                    return BadRequest(new
                    {
                        status = "Email_Aleady_Exists",
                        message = "Email address must be Unique"
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
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.FindSingleAsync(filter: f => f.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _unitOfWork.Users.Delete(user);
            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                Message = "User deleted"
            });
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
    }
}




// Alert: {
//"error": "An unexpected error occurred. Please try again later.",
//    "details": "A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently
//    using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913."
//}


// I want you to create a CustomControllerBase that will override the BadRequest, so it will return something like this:         return Problem(detail: message, statusCode: StatusCodes.Status400BadRequest, title: status);

