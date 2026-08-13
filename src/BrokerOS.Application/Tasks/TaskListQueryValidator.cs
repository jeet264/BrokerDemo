using FluentValidation;

namespace BrokerOS.Application.Tasks;

public sealed class TaskListQueryValidator : AbstractValidator<TaskListQuery>
{
    private static readonly string[] Views =
    [
        "mine",
        "team",
        "overdue",
        "completed"
    ];

    public TaskListQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.SortBy).MaximumLength(50);
        RuleFor(query => query.SortDir).MaximumLength(10);
        RuleFor(query => query.View)
            .Must(value => Views.Contains(value!.Trim().ToLowerInvariant()))
            .When(query => !string.IsNullOrWhiteSpace(query.View))
            .WithMessage("Task view is not supported.");
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Priority).IsInEnum().When(query => query.Priority.HasValue);
        RuleFor(query => query.ToDate)
            .GreaterThanOrEqualTo(query => query.FromDate!.Value)
            .When(query => query.FromDate.HasValue && query.ToDate.HasValue);
    }
}
