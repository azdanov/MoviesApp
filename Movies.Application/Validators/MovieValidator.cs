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

        RuleFor(movie => movie.Id)
            .NotEmpty();
        RuleFor(movie => movie.Title)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(movie => movie.YearOfRelease)
            .NotEmpty()
            .GreaterThanOrEqualTo(1878)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(movie => movie.Genres)
            .Must(genres => genres.Any()).WithMessage("A movie must have at least 1 genre.")
            .Must(genres => genres.Count <= 5).WithMessage("A movie can have a maximum of 5 genres.");
        RuleFor(movie => movie.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This movie already exists.");
    }

    private async Task<bool> ValidateSlug(Movie movie, string movieSlug, CancellationToken cancellationToken = default)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(movieSlug, cancellationToken: cancellationToken);

        if (existingMovie is null) return true;
        if (existingMovie.Id == movie.Id) return true;

        return false;
    }
}