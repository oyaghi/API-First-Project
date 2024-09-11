using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_First_Project.Mappers;
using API_First_Project.Commands;
using API_First_Project.Dtos;
using API_First_Project.Models;
using API_First_Project.IUnitOfWork;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWorks unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWorks _unitOfWork = unitOfWork;

        // GET: api/Users
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {

            var users = await _unitOfWork.Users.GetAsync();
            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }

            var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();
            return Ok(userDtos);
        }

        // GET: api/Users/1
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(UsersMapper.ToUserDto(user));

        }

        // POST: api/Users
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {

            // Add Unique PhoneNumber Validation
            var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
            if (phoneExists.Any())
            {
                return BadRequest();
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

        // PUT: api/Users/1
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
                var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
                if (phoneExists.Any())
                {
                    return BadRequest();
                }
            }

            if (user.Email != command.Email)
            {
                var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == command.Email);
                if (emailExists.Any())
                {
                    return BadRequest();
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
            return Ok("User successfully deleted.");

        }
    }
}