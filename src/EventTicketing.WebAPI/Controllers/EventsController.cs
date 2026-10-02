using EventTicketing.Business.Abstract;
using EventTicketing.Core.Utilities.Results;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketing.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private const string AdminRoles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Organizer)}";
    private static readonly string[] AllowedPosterExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly IEventService _eventService;
    private readonly IReportingService _reportingService;
    private readonly IWebHostEnvironment _environment;

    public EventsController(
        IEventService eventService, IReportingService reportingService, IWebHostEnvironment environment)
    {
        _eventService = eventService;
        _reportingService = reportingService;
        _environment = environment;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<EventListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var includeDrafts =
            User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.Organizer));

        return Ok(await _eventService.GetListAsync(page, pageSize, includeDrafts, ct));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await _eventService.GetDetailAsync(id, ct));

    [HttpGet("{id:int}/sections")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SectionAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSections(int id, CancellationToken ct) =>
        Ok(await _reportingService.GetSectionAvailabilityAsync(id, ct));

    [HttpGet("{id:int}/seats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AvailableSeatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSeats(
        int id, [FromQuery] int? eventSectionId, CancellationToken ct) =>
        Ok(await _reportingService.GetAvailableSeatsAsync(id, eventSectionId, ct));

    [HttpGet("{id:int}/sections/{eventSectionId:int}/remaining")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGeneralAdmissionRemaining(
        int id, int eventSectionId, CancellationToken ct) =>
        Ok(await _reportingService.GetGeneralAdmissionRemainingAsync(eventSectionId, ct));

    [HttpGet("venues")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(List<SimpleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVenues(CancellationToken ct) =>
        Ok(await _eventService.GetVenuesAsync(ct));

    [HttpGet("organizers")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(List<SimpleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizers(CancellationToken ct) =>
        Ok(await _eventService.GetOrganizersAsync(ct));

    [HttpGet("venues/{venueId:int}/seat-categories")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(List<SimpleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSeatCategories(int venueId, CancellationToken ct) =>
        Ok(await _eventService.GetSeatCategoriesAsync(venueId, ct));

    [HttpGet("{id:int}/admin")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(AdminEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdminDetail(int id, CancellationToken ct) =>
        Ok(await _eventService.GetAdminDetailAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateEventRequest request, CancellationToken ct)
    {
        var id = await _eventService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAdminDetail), new { id }, id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request, CancellationToken ct)
    {
        await _eventService.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _eventService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/prices")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrices(int id, SetPricesRequest request, CancellationToken ct)
    {
        await _eventService.SetPricesAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        await _eventService.PublishAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/poster")]
    [Authorize(Roles = AdminRoles)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(int id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            throw new BusinessRuleException("The uploaded file is empty.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedPosterExtensions.Contains(extension))
            throw new BusinessRuleException("Only jpg, png and webp posters are allowed.");

        var posterDirectory = Path.Combine(_environment.ContentRootPath, "wwwroot", "posters");
        Directory.CreateDirectory(posterDirectory);

        var fileName = $"{id}{extension}";
        var filePath = Path.Combine(posterDirectory, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream, ct);

        var posterUrl = $"/posters/{fileName}";
        await _eventService.SetPosterAsync(id, posterUrl, ct);

        return Ok(posterUrl);
    }
}
