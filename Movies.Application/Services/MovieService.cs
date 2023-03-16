using System.Transactions;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

internal class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public Task<bool> CreateAsync(Movie movie)
    {
        return _movieRepository.CreateAsync(movie);
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        return _movieRepository.GetByIdAsync(id);
    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        return _movieRepository.GetBySlugAsync(slug);
    }

    public Task<IEnumerable<Movie>> GetAllAsync()
    {
        return _movieRepository.GetAllAsync();
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id);
        if (!movieExists) return null;

        var result = await _movieRepository.UpdateAsync(movie);
        if (!result) return null;

        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        return _movieRepository.DeleteByIdAsync(id);
    }
}