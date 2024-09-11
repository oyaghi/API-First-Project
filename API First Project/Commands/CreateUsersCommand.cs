using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace API_First_Project.Commands
{
    public class CreateUsersCommand
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(155)]
        [MinLength(3)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MaxLength(155)]
        [MinLength(3)]
        public string Lastname { get; set; } = string.Empty;
        [Required]
        [EnumDataType(typeof(Gender))]
        public Gender Gender { get; set; }

        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
