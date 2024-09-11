using Core.Models;
using API_First_Project.Dtos;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace API_First_Project.Mappers
{
    public class UsersMapper
    {
        public static UserDto ToUserDto(User user)
        {
            return new UserDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                PhoneNumber = user.PhoneNumber,
            };
        }
    }
}
