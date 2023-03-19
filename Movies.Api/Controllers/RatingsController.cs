using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0, Deprecated = true)]
[ApiVersion(1.1)]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly IOutputCacheStore _outputCacheStore;

    public RatingsController(IRatingService ratingService, IOutputCacheStore outputCacheStore)
    {
        _ratingService = ratingService;
        _outputCacheStore = outputCacheStore;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateMovieAsync([FromRoute] Guid movieId, [FromBody] RateMovieRequest request,
        CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var options = request.MapToOptions()
            .WithMovieId(movieId)
            .WithUserId(userId!.Value);

        var result = await _ratingService.RateMovieAsync(options, token);
        if (!result) return NotFound();
        
        await _outputCacheStore.EvictByTagAsync("Ratings", token);

        return NoContent();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid movieId,
        CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, token);
        if (!result) return NotFound();
        
        await _outputCacheStore.EvictByTagAsync("Ratings", token);

        return NoContent();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    [OutputCache(Duration = 10)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept, Accept-Encoding")]
    [ProducesResponseType(typeof(IEnumerable<MovieRating>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRatingsAsync(CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);

        var response = ratings.MapToResponse();
        return Ok(response);
    }
}