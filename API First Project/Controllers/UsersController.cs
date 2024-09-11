using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_First_Project.Mappers;
using API_First_Project.Commands;
using API_First_Project.IUnitOfWork;
using API_First_Project.Dtos;
using API_First_Project.Models;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWorks _unitOfWork;

        public UsersController(IUnitOfWorks unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Users
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();
                return Ok(userDtos);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users.");
            }
        }

        // GET: api/Users/1
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found. Check the ID and try again.");
                }

                return Ok(UsersMapper.ToUserDto(user));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user.");
            }
        }

        // POST: api/Users
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {
            try
            {
                // Add Unique PhoneNumber Validation
                var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
                if (phoneExists.Any())
                {
                    return BadRequest("Phone number already exists.");
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
                await _unitOfWork.CompleteAsync();

                var userDto = UsersMapper.ToUserDto(user);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user.");
            }
        }

        // PUT: api/Users/1
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found. Check the ID and try again.");
                }

                if (user.PhoneNumber != command.PhoneNumber)
                {
                    var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
                    if (phoneExists.Any())
                    {
                        return BadRequest("Phone number already exists.");
                    }
                }

                if (user.Email != command.Email)
                {
                    var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == command.Email);
                    if (emailExists.Any())
                    {
                        return BadRequest("Email already exists.");
                    }
                }

                user.FirstName = command.FirstName;
                user.Lastname = command.Lastname;
                user.PhoneNumber = command.PhoneNumber;
                user.Gender = command.Gender;
                user.Email = command.Email;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                return Ok(UsersMapper.ToUserDto(user));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user.");
            }
        }

        // DELETE: api/Users/1
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found. Check the ID and try again.");
                }

                _unitOfWork.Users.Delete(user);
                await _unitOfWork.CompleteAsync();
                return Ok("User successfully deleted.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user.");
            }
        }
    }
}
