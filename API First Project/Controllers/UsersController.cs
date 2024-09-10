using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_First_Project.Data;
using API_First_Project.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using API_First_Project.Mappers;
using API_First_Project.Commands;
using API_First_Project.IUnitOfWork;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController: ControllerBase
    {

        private readonly IUnitOfWorks _unitOfWork;

        public UsersController(IUnitOfWorks unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await  _unitOfWork.Users.GetAllAsync();
            var userDtos = users.Select(user => UsersMapper.ToUserDto(user)).ToList();

            return Ok(userDtos);
        }
        /// <summary>
        /// Returns the specified user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Ok200 user</returns>
        // GET: api/Users/1
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(UsersMapper.ToUserDto(user));

        }
        /// <summary>
        /// Create a new User 
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Created user information</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUsersCommand command)
        {
            // Add Unique Email Validation
            var phoneExists = await _unitOfWork.Users.FindAsync(u => u.PhoneNumber == command.PhoneNumber);
            if (phoneExists!= null)
            {
                return BadRequest("PhneNumber already exists in the database");
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

            return Ok(user);
        }

        /// <summary>
        /// Update the user partially and Fully
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns>Ok200 if the user updated successfully</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.PhoneNumber != command.PhoneNumber)
            {
                var phoneExists = await _unitOfWork.Users.FindAsync(u=> u.PhoneNumber == command.PhoneNumber);
                if (phoneExists!= null)
                {
                    return BadRequest("PhneNumber already exists in the database");
                }
            }


            if (user.Email != command.Email)
            {
                var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == command.Email);
                if (emailExists != null )
                {
                    return BadRequest("Email already exists in the database");
                }
            }

            user.FirstName = command.FirstName;
            user.Lastname = command.Lastname;
            user.PhoneNumber = command.PhoneNumber;
            user.Gender = command.Gender;
            user.Email = command.Email;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return Ok(user);


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _unitOfWork.Users.Delete(user);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }

    }
}
