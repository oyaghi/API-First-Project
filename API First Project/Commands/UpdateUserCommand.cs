using Core.Enums;
using Core.IUnitOfWork;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace API_First_Project.Commands
{
    public class UpdateUserCommand
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


    public class UpdateUsersCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUsersCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(command => command.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(command => command.Lastname)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(command => command.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(PhoneNumberNotExists).WithMessage("Phone number already exists in the database.");

            RuleFor(command => command.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .Must(EmailAddressNotExists).WithMessage("Email address already exists in the database.");
        }

        private bool PhoneNumberNotExists(string phoneNumber)
        {
            // Assuming synchronous method in your repository
            var phoneExists = _unitOfWork.Users.FindSingleAsync(u => u.PhoneNumber == phoneNumber);
            return phoneExists == null;
        }

        private bool EmailAddressNotExists(string email)
        {
            // Assuming synchronous method in your repository
            var emailExists = _unitOfWork.Users.FindSingleAsync(u => u.Email == email);
            return emailExists == null;
        }

    }
}
