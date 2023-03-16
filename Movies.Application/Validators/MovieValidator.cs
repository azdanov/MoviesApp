using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;

    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(x => x.Id)
            .NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.YearOfRelease)
            .NotEmpty()
            .GreaterThanOrEqualTo(1878)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(x => x.Genres)
            .Must(genres => genres.Any()).WithMessage("A movie must have at least 1 genre.")
            .Must(genres => genres.Count <= 5).WithMessage("A movie can have a maximum of 5 genres.");
        RuleFor(x => x.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This movie already exists.");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug, token);

        if (existingMovie is null) return true;
        if (existingMovie.Id == movie.Id) return true;

        return false;
    }
}