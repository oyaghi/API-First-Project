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
            var users= await _context.users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] int Id)
        {
            var user = await _context.users.SingleOrDefaultAsync(x => x.Id == Id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);

        }
    }
}
