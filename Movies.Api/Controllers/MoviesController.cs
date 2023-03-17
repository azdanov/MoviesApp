using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();

        var created = await _movieService.CreateAsync(movie, cancellationToken);
        if (!created) return Conflict();

        var response = movie.MapToMovieResponse();
        return CreatedAtAction(nameof(GetById), new { movieIdOrSlug = movie.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.GetById)]
    public async Task<IActionResult> GetById([FromRoute] string movieIdOrSlug, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(movieIdOrSlug, out var movieId)
            ? await _movieService.GetByIdAsync(movieId, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(movieIdOrSlug, userId, cancellationToken);
        if (movie is null) return NotFound();

        var response = movie.MapToMovieResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movies = await _movieService.GetAllAsync(userId, cancellationToken);

        var response = movies.MapToMoviesResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid movieId, [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(movieId);

        var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updatedMovie is null) return NotFound();

        var response = updatedMovie.MapToMovieResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid movieId, CancellationToken cancellationToken)
    {
        var deleted = await _movieService.DeleteByIdAsync(movieId, cancellationToken);
        if (!deleted) return NotFound();

        return NoContent();
    }
}