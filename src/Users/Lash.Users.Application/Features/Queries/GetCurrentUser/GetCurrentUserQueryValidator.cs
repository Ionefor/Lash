using FluentValidation;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Features.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.UserIdRequired)
            .WithMessage("User identifier must be provided.");
    }
}
