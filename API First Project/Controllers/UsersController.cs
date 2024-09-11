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

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {
            var users = await _unitOfWork.Users.GetAsync();

            var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();
            return Ok(new
            {
                Users = userDtos,
                UserWithTenant = userDtos,
                Status = "Haaasdas"
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

            return Ok(UsersMapper.ToUserDto(user));

        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {

            // Add Unique PhoneNumber Validation
            var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
            if (phoneExists.Any())
            {
                //return BadRequest("PHONE_ALREADY_EXSIST", "Phone number exists in the database, it has to be unique");

                return BadRequest(new
                {
                    Message = "Validation failed.",
                    Details = "Review the errors and correct them",
                    Errors = new Dictionary<string, string>
                    {
                        { "phoneNumber", "Phone number exists in the database, it has to be unique" },
                    }
                });
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
                    return BadRequest(new
                    {
                        Message = "Validation failed.",
                        Details = "Review the errors and correct them",
                        Errors = new Dictionary<string, string>
                        {
                            { "phoneNumber", "Phone number exists in the database, it has to be unique" }
                        }
                    });
                }
            }

            if (user.Email != command.Email)
            {
                var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == command.Email);
                if (emailExists.Any())
                {
                    return BadRequest(new
                    {
                        Message = "Validation failed.",
                        Details = "Review the errors and correct them",
                        Errors = new Dictionary<string, string>
                        {
                            { "Email", "Email address exists in the database, it has to be unique" }
                        }
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

            return Ok(UsersMapper.ToUserDto(user));
        }

        // DELETE: api/Users/1
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

            return Ok();
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
    }
}




//TODO: Override the Http response handler
//TODO: Edit the Get to let it sort results based on columns