using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace API_First_Project.Dtos
{
    public class UserDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(155)]
        [MinLength(3)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
