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

        public CreateUsersCommandValidator()
        {

            RuleFor(command => command.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(command => command.Lastname)
                .NotEmpty().WithMessage("Last name is required.");
        }
    }
}




