using AutoMapper;
using EventTicketing.Business.Abstract;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Enums;
using EventTicketing.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IReportingService _reportingService;
    private readonly IMapper _mapper;

    public TicketsController(
        ITicketService ticketService, IReportingService reportingService, IMapper mapper)
    {
        _ticketService = ticketService;
        _reportingService = reportingService;
        _mapper = mapper;
    }

    [HttpPost("purchase/reserved-seat")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PurchaseReservedSeat(
        PurchaseReservedSeatRequest request, CancellationToken ct)
    {
        var ticket = await _ticketService.PurchaseReservedSeatAsync(
            request.EventId, request.EventSeatId, User.GetUserId(), ct);

        return CreatedAtAction(nameof(GetMine), null, _mapper.Map<TicketDto>(ticket));
    }

    [HttpPost("purchase/general-admission")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PurchaseGeneralAdmission(
        PurchaseGeneralAdmissionRequest request, CancellationToken ct)
    {
        var ticket = await _ticketService.PurchaseGeneralAdmissionAsync(
            request.EventId, request.EventSectionId, User.GetUserId(), ct);

        return CreatedAtAction(nameof(GetMine), null, _mapper.Map<TicketDto>(ticket));
    }

    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(long id, CancelTicketRequest request, CancellationToken ct)
    {
        await _ticketService.CancelAsync(id, User.GetUserId(), request.Reason, ct);
        return NoContent();
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(List<UserTicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct) =>
        Ok(await _reportingService.GetUserTicketHistoryAsync(User.GetUserId(), ct));

    [HttpGet("user/{userId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(List<UserTicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(int userId, CancellationToken ct) =>
        Ok(await _reportingService.GetUserTicketHistoryAsync(userId, ct));
}
