using Core.Enums;
using Core.IUnitOfWork;
using Core.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API_First_Project.Commands
{
    public class CreateUsersCommand
    {


        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string Lastname { get; set; } = string.Empty;

        [EnumDataType(typeof(Gender))]
        public Gender Gender { get; set; }


        public string PhoneNumber { get; set; } = string.Empty;
    }
    public class CreateUsersCommandValidator : AbstractValidator<CreateUsersCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUsersCommandValidator(IUnitOfWork unitOfWork)
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




