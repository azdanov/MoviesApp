using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class RateMovieOptionsValidator : AbstractValidator<RateMovieOptions>
{
    public RateMovieOptionsValidator()
    {
        RuleFor(rateMovie => rateMovie.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
    }
}