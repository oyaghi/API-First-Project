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
using NuGet.Configuration;
using System.Text.Json;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using FluentValidation;
using FluentValidation.Results;

namespace API_First_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUnitOfWork unitOfWork, ITenantService tenantService, IValidator<CreateUsersCommand> createUsersCommandValidator) : CustomControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITenantService _tenantService = tenantService;
        private readonly IValidator<CreateUsersCommand> _createUsersCommandValidator = createUsersCommandValidator;


        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(int pageNumber, int pageSize)
        {
            var pagedObjectUsers = await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize);

            var mappedUsers = pagedObjectUsers.Results
                .Select(user => UsersMapper.ToUserDto(user))
                .ToList();

            var response = new
            {
                Users = mappedUsers,
                pagedObjectUsers.CurrentPage,
                pagedObjectUsers.PageSize,
                pagedObjectUsers.RowCount,
                pagedObjectUsers.PageCount
            };

            return Ok(response);
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
            if (command == null)
            {
                return BadRequest(new {
                    title = "Bad Request",
                    detail = "No JSON object provided",
                    statusCode = 400

                });
            }
            ValidationResult result = _createUsersCommandValidator.Validate(command);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                    .ToList();

                return BadRequest(new {
                    title = "Bad Request",
                    detail = errors,
                    statusCode = 400
                });
            }

            var phoneExists = await _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == command.PhoneNumber);
            if (phoneExists != null)
            {
                return BadRequest(new
                {
                    title = "Phone_Already_Exists",
                    detail = "Phone number must be Unique",
                    statusCode = 400

                });
            }

            var emailExists = await _unitOfWork.Users.FindSingleAsync(u => u.Email == command.Email);
            if (emailExists != null)
            {
                return BadRequest(new
                {
                    title = "Email_Aleady_Exists",
                    detail = "Email address must be Unique",
                    statusCode = 400
                });
            }

            var user = new User
            {
                Email = command.Email,
                FirstName = command.FirstName,
                Lastname = command.Lastname,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                Setting = new User.Settings
                {
                    Language = command.Setting.Language,
                    Color = command.Setting.Color,
                    Theme = command.Setting.Theme
                }
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
                        title = "Phone_Already_Exists",
                        detail = "Phone number must be Unique",
                        statusCode = 400
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
                        title = "Email_Aleady_Exists",
                        detail = "Email address must be Unique",
                        statusCode = 400
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

            return Ok();
        }


        [Authorize]
        [HttpGet("JsonTest")]
        [ProducesResponseType(typeof(List<UserSettingDto>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<UserSettingDto>> GetJson()
        {
            var users = await _unitOfWork.Users.GetAsync();

            var filteredUsers = users.Where(u => u.Setting.Language == "Arabic").ToList();

            var userDtos = filteredUsers.Select(u => new UserSettingDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                Lastname = u.Lastname,
                Setting = new UserSettingDto.SettingDto 
                {
                    Language = u.Setting.Language,
                    Color = u.Setting.Color,
                    Theme = u.Setting.Theme
                }
            }).ToList();

            return userDtos;
        }

    }
}
