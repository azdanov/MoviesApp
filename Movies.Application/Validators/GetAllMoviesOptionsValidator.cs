using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptedSortFields = { "title", "yearOfRelease", "rating" };

    public GetAllMoviesOptionsValidator()
    {
        RuleFor(options => options.Title)
            .MaximumLength(100);
        RuleFor(options => options.YearOfRelease)
            .GreaterThanOrEqualTo(1878)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(options => options.SortField)
            .Must(field => AcceptedSortFields.Contains(field, StringComparer.OrdinalIgnoreCase))
            .When(options => !string.IsNullOrWhiteSpace(options.SortField))
            .WithMessage($"You can only sort by {string.Join(", ", AcceptedSortFields)}");
    }
}