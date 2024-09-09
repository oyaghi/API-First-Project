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

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(TestingDbContext context) : ControllerBase
    {

        private readonly TestingDbContext _context = context;

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.users.ToListAsync();
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
            var user = await _context.users.SingleOrDefaultAsync(x => x.Id == id);
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
            if (_context.users.Any(u => u.PhoneNumber == command.PhoneNumber))
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

            await _context.users.AddAsync(user);
            _context.SaveChanges();

            return Ok(user);
        }

        /// <summary>
        /// Update the user partially and Fully
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns>Ok200 if the user updated successfully</returns>
        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] int id, [FromBody] UpdateUserCommand command)
        {
            var user = _context.users.SingleOrDefault(x => x.Id == id);
            if (user == null)
            {

                return NotFound();
            }

            if (user.PhoneNumber != command.PhoneNumber)
            {
                if (_context.users.Any(u => u.PhoneNumber == command.PhoneNumber))
                {
                    return BadRequest("PhneNumber already exists in the database");
                }
            }


            if (user.Email != command.Email)
            {
                if (_context.users.Any(u => u.Email == command.Email))
                {
                    return BadRequest("Email already exists in the database");
                }
            }

            user.FirstName = command.FirstName;
            user.Lastname = command.Lastname;
            user.PhoneNumber = command.PhoneNumber;
            user.Gender = command.Gender;
            user.Email = command.Email;

            //_context.Update(user);
            _context.SaveChanges();
            return Ok(user);


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await _context.users.SingleAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Remove(user);
            _context.SaveChanges();
            return Ok();
        }

    }
}
