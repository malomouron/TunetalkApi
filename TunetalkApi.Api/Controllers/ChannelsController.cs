using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TunetalkApi.Application.Channels;
using TunetalkApi.Application.Chat;

namespace TunetalkApi.Api.Controllers;

[ApiController]
[Route("api/channels")]
public sealed class ChannelsController(
    IRadioChannelCatalog channelCatalog,
    ChatMessageService chatMessageService,
    ILogger<ChannelsController> logger) : ControllerBase
{
    [HttpGet("genres")]
    [ProducesResponseType<IReadOnlyList<ChannelGenre>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ChannelGenre>> GetGenres() => Ok(ChannelGenres.All);

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RadioChannel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetByGenre(
        [FromQuery] string? genre,
        CancellationToken cancellationToken)
    {
        var selectedGenre = ChannelGenres.Find(genre);
        if (selectedGenre is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unsupported genre",
                Detail = "Choose a genre returned by GET /api/channels/genres.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var channels = await channelCatalog.GetByGenreAsync(selectedGenre.Id, cancellationToken);
            return Ok(channels);
        }
        catch (CatalogUnavailableException exception)
        {
            logger.LogWarning(exception, "The external radio catalog is unavailable.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Radio catalog unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status503ServiceUnavailable
            });
        }
    }

    [Authorize]
    [HttpGet("{channelId}/messages")]
    [ProducesResponseType<IReadOnlyList<ChatMessageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentMessages(
        string channelId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(channelId, out _))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid channel ID",
                Detail = "A radio station UUID is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var messages = await chatMessageService.GetRecentAsync(channelId, cancellationToken);
        return Ok(messages);
    }
}
