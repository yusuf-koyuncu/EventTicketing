using EventTicketing.Business.Abstract;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Organizer)}")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("events/{eventId:int}/occupancy")]
    [ProducesResponseType(typeof(EventOccupancyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOccupancy(int eventId, CancellationToken ct) =>
        Ok(await _reportingService.GetOccupancyAsync(eventId, ct));

    [HttpGet("events/{eventId:int}/revenue")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRealisedRevenue(int eventId, CancellationToken ct) =>
        Ok(await _reportingService.GetRealisedRevenueAsync(eventId, ct));
}
