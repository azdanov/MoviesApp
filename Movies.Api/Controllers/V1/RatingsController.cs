using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers.V1;

[ApiController]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.V1.Movies.Rate)]
    public async Task<IActionResult> RateMovieAsync([FromRoute] Guid movieId, [FromBody] RateMovieRequest request,
        CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var options = request.MapToOptions()
            .WithMovieId(movieId)
            .WithUserId(userId!.Value);

        var result = await _ratingService.RateMovieAsync(options, token);
        if (!result) return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.V1.Movies.DeleteRating)]
    public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid movieId, CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, token);
        if (!result) return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.V1.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatingsAsync(CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();

        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);

        var response = ratings.MapToResponse();
        return Ok(response);
    }
}