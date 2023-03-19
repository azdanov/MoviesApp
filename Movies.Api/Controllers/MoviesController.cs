using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0, Deprecated = true)]
[ApiVersion(1.1)]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IOutputCacheStore _outputCacheStore;

    public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    {
        _movieService = movieService;
        _outputCacheStore = outputCacheStore;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken token)
    {
        var movie = request.MapToMovie();

        var created = await _movieService.CreateAsync(movie, token);
        if (!created) return Conflict();

        await _outputCacheStore.EvictByTagAsync("Movies", token);

        var response = movie.MapToResponse();
        return CreatedAtAction(nameof(GetByIdV2), new { movieIdOrSlug = movie.Id }, response);
    }

    [MapToApiVersion(1.0)]
    [HttpGet(ApiEndpoints.Movies.GetById)]
    [OutputCache(PolicyName = "MoviesCache")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept, Accept-Encoding")]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [OutputCache(PolicyName = "MoviesCache")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept, Accept-Encoding")]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [OutputCache(PolicyName = "MoviesCache",
        VaryByQueryKeys = new[] { "page", "pageSize", "title", "yearOfRelease", "sortBy" })]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept, Accept-Encoding",
        VaryByQueryKeys = new[] { "page", "pageSize", "title", "yearOfRelease", "sortBy" })]
    [ProducesResponseType(typeof(PagedResponse<MovieResponse>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid movieId, [FromBody] UpdateMovieRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(movieId);

        var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
        if (updatedMovie is null) return NotFound();

        await _outputCacheStore.EvictByTagAsync("Movies", token);

        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid movieId, CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(movieId, token);
        if (!deleted) return NotFound();

        await _outputCacheStore.EvictByTagAsync("Movies", token);

        return NoContent();
    }
}