using Dapper;
using KrusaClub.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrusaClub.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HealthController(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet("db")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckDatabase(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition(
                "select 1;",
                cancellationToken: cancellationToken));

            return Ok(new
            {
                status = "ok",
                database = "connected",
                result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = "error",
                database = "disconnected",
                error = ex.Message
            });
        }
    }
}
