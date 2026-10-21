using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TunetalkApi.Application.Identity;

namespace TunetalkApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("authentication")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RegisterAsync(
            request.DisplayName,
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(result.Errors.ToDictionary()));
        }

        return StatusCode(StatusCodes.Status201Created, AuthResponse.From(result.Authentication!));
    }

    [HttpPost("login")]
    [EnableRateLimiting("authentication")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Detail = "Email or password is incorrect.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(AuthResponse.From(result.Authentication!));
    }
}

public sealed class RegisterRequest
{
    [Required, StringLength(40, MinimumLength = 2)]
    public string DisplayName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string Email,
    string DisplayName)
{
    public static AuthResponse From(AuthenticationResponse response) =>
        new(response.AccessToken, response.ExpiresAt, response.Email, response.DisplayName);
}
