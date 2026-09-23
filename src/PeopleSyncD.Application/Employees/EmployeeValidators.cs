using FluentValidation;

namespace PeopleSyncD.Application.Employees;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(request => request.EmployeeNumber).NotEmpty().MaximumLength(64);
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Department).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Location).NotEmpty().MaximumLength(200);
        RuleFor(request => request.EmploymentType).IsInEnum();
        RuleFor(request => request.StartDate).NotEqual(default(DateOnly));
    }
}

public sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(request => request.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Department).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Location).NotEmpty().MaximumLength(200);
        RuleFor(request => request.EmploymentType).IsInEnum();
    }
}

public sealed class ChangeEmploymentStatusValidator : AbstractValidator<ChangeEmploymentStatusRequest>
{
    public ChangeEmploymentStatusValidator()
    {
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.SeparationDate)
            .NotNull()
            .When(request => request.Status == PeopleSyncD.Domain.Employees.EmploymentStatus.Separated);
    }
}
