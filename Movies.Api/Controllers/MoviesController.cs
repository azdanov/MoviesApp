using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0, Deprecated = true), ApiVersion(1.1)]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken token)
    {
        var movie = request.MapToMovie();

        var created = await _movieService.CreateAsync(movie, token);
        if (!created) return Conflict();

        var response = movie.MapToResponse();
        return CreatedAtAction(nameof(GetByIdV2), new { movieIdOrSlug = movie.Id }, response);
    }

    [MapToApiVersion(1.0)]
    [HttpGet(ApiEndpoints.Movies.GetById)]
    public async Task<IActionResult> GetByIdV1([FromRoute] string movieIdOrSlug, CancellationToken token)
    {
        await Task.Delay(1000, token);
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(movieIdOrSlug, out var movieId)
            ? await _movieService.GetByIdAsync(movieId, userId, token)
            : await _movieService.GetBySlugAsync(movieIdOrSlug, userId, token);
        if (movie is null) return NotFound();

        var response = movie.MapToResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetById)]
    public async Task<IActionResult> GetByIdV2([FromRoute] string movieIdOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(movieIdOrSlug, out var movieId)
            ? await _movieService.GetByIdAsync(movieId, userId, token)
            : await _movieService.GetBySlugAsync(movieIdOrSlug, userId, token);
        if (movie is null) return NotFound();

        var response = movie.MapToResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();

        var options = request.MapToOptions()
            .WithUserId(userId);

        var movies = await _movieService.GetAllAsync(options, token);
        var moviesCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);

        var response = movies.MapToResponse(request.Page, request.PageSize, moviesCount);
        return Ok(response);
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid movieId, [FromBody] UpdateMovieRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(movieId);

        var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
        if (updatedMovie is null) return NotFound();

        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid movieId, CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(movieId, token);
        if (!deleted) return NotFound();

        return NoContent();
    }
}