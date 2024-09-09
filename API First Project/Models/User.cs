using API_First_Project.Enums;
using System.ComponentModel.DataAnnotations;
namespace API_First_Project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MaxLength(155)]
        [MinLength(155)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MaxLength(155)]
        [MinLength(155)]
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
