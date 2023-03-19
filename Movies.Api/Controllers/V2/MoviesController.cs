using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;

namespace Movies.Api.Controllers.V2;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet(ApiEndpoints.V2.Movies.GetById)]
    public async Task<IActionResult> GetById([FromRoute] string movieIdOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(movieIdOrSlug, out var movieId)
            ? await _movieService.GetByIdAsync(movieId, userId, token)
            : await _movieService.GetBySlugAsync(movieIdOrSlug, userId, token);
        if (movie is null) return NotFound();

        var response = movie.MapToResponse();
        return Ok(response);
    }
}