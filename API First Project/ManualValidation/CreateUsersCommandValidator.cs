using API_First_Project.Commands;
using FluentValidation;
using System.Threading.Tasks;

public class CreateUsersCommandValidator : AbstractValidator<CreateUsersCommand>
{
    public CreateUsersCommandValidator( )
    {

        // Email validation
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        // FirstName validation
        RuleFor(command => command.FirstName)
            .NotEmpty().WithMessage("First Name is required.")
            .MaximumLength(50).WithMessage("First Name must be at most 50 characters.");

        // LastName validation
        RuleFor(command => command.Lastname)
            .NotEmpty().WithMessage("Last Name is required.")
            .MaximumLength(50).WithMessage("Last Name must be at most 50 characters.");

        // Gender validation
        RuleFor(command => command.Gender)
            .IsInEnum().WithMessage("Gender must be a valid enum value.");

        // PhoneNumber validation
        RuleFor(command => command.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required.")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format.");

        // Settings validation
        RuleFor(command => command.Setting)
            .NotNull().WithMessage("Settings cannot be null.")
            .SetValidator(new SettingsValidator());
    }

    public class SettingsValidator : AbstractValidator<CreateUsersCommand.Settings>
    {
        public SettingsValidator()
        {
            RuleFor(settings => settings.Language)
                .NotEmpty().WithMessage("Language is required.");

            RuleFor(settings => settings.Color)
                .NotEmpty().WithMessage("Color is required.");

            RuleFor(settings => settings.Theme)
                .NotEmpty().WithMessage("Theme is required.");
        }
    }
}

/*
MustAsync 
SetValidatorAsync
CustomAsync
WhenAsync
 */