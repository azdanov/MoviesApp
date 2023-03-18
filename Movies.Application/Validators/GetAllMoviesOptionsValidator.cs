using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(movie => movie.Title)
            .MaximumLength(100);
        RuleFor(movie => movie.YearOfRelease)
            .GreaterThanOrEqualTo(1878)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
    }
}