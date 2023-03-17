using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    public async Task<IActionResult> RateMovieAsync([FromRoute] Guid movieId, [FromBody] RateMovieRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();

        var result = await _ratingService.RateMovieAsync(movieId, userId!.Value, request.Rating, cancellationToken);
        if (!result) return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid movieId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();

        var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, cancellationToken);
        if (!result) return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatingsAsync(CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();

        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);

        var response = ratings.MapToMovieRatingsResponse();
        return Ok(response);
    }
}