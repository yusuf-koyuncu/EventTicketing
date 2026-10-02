using EventTicketing.Business.Abstract;
using EventTicketing.Core.DataAccess;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.Business.Concrete;

public class TicketManager : ITicketService
{
    private readonly ITicketDal _ticketDal;
    private readonly IEventDal _eventDal;
    private readonly IUnitOfWork _unitOfWork;

    public TicketManager(ITicketDal ticketDal, IEventDal eventDal, IUnitOfWork unitOfWork)
    {
        _ticketDal = ticketDal;
        _eventDal = eventDal;
        _unitOfWork = unitOfWork;
    }

    public Task<Ticket> PurchaseReservedSeatAsync(
        int eventId, long eventSeatId, int userId, CancellationToken ct = default) =>
        _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var nowUtc = DateTime.UtcNow;

            var @event = await GetSellableEventAsync(eventId, nowUtc, token);

            var eventSeat = await _ticketDal.LockEventSeatAsync(eventId, eventSeatId, token)
                ?? throw new SeatUnavailableException(
                    $"Seat {eventSeatId} is not part of event {eventId}.");

            if (eventSeat.Status == EventSeatStatus.Blocked)
                throw new SeatUnavailableException(
                    $"Seat {eventSeatId} is blocked: {eventSeat.BlockReason}.");

            if (await _ticketDal.HasActiveTicketForSeatAsync(eventSeatId, token))
                throw new SeatUnavailableException(
                    $"Seat {eventSeatId} is already sold for event {eventId}.");

            var seatInfo = await _ticketDal.GetSeatPurchaseInfoAsync(eventSeatId, token);

            if (!seatInfo.IsSectionOnSale)
                throw new BusinessRuleException($"Section {seatInfo.SectionName} is not on sale.");

            var price = await _eventDal.GetActivePriceAsync(eventId, seatInfo.SeatCategoryId, token)
                ?? throw new BusinessRuleException(
                    $"No active price for event {eventId}, category {seatInfo.SeatCategoryId}.");

            var label = seatInfo.RowLabel is null
                ? $"{seatInfo.SectionName} / Seat {seatInfo.SeatNumber}"
                : $"{seatInfo.SectionName} / Row {seatInfo.RowLabel} / Seat {seatInfo.SeatNumber}";

            var ticket = Ticket.Purchase(
                eventId: eventId,
                eventSectionId: seatInfo.EventSectionId,
                eventSeatId: eventSeatId,
                seatCategoryId: seatInfo.SeatCategoryId,
                userId: userId,
                price: price.Price,
                serviceFee: price.ServiceFee,
                currency: @event.Currency,
                seatLabel: label,
                nowUtc: nowUtc);

            await _ticketDal.SaveNewTicketAsync(ticket, token);
            return ticket;
        }, ct);

    public Task<Ticket> PurchaseGeneralAdmissionAsync(
        int eventId, int eventSectionId, int userId, CancellationToken ct = default) =>
        _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var nowUtc = DateTime.UtcNow;

            var @event = await GetSellableEventAsync(eventId, nowUtc, token);

            var section = await _ticketDal.LockEventSectionAsync(eventId, eventSectionId, token)
                ?? throw new SeatUnavailableException(
                    $"Section {eventSectionId} is not part of event {eventId}.");

            if (!section.IsOnSale)
                throw new BusinessRuleException($"Section {eventSectionId} is not on sale.");

            var capacity = section.GeneralAdmissionCapacity
                ?? throw new BusinessRuleException(
                    $"Section {eventSectionId} is reserved seating; use {nameof(PurchaseReservedSeatAsync)}.");

            var sold = await _ticketDal.CountActiveTicketsInSectionAsync(eventSectionId, token);

            if (sold >= capacity)
                throw new CapacityExceededException(
                    $"Section {eventSectionId} is sold out ({sold}/{capacity}).");

            var sectionName = await _ticketDal.GetSectionNameAsync(eventSectionId, token);

            var price = await _eventDal.GetActivePriceAsync(eventId, section.SeatCategoryId, token)
                ?? throw new BusinessRuleException(
                    $"No active price for event {eventId}, category {section.SeatCategoryId}.");

            var ticket = Ticket.Purchase(
                eventId: eventId,
                eventSectionId: eventSectionId,
                eventSeatId: null,
                seatCategoryId: section.SeatCategoryId,
                userId: userId,
                price: price.Price,
                serviceFee: price.ServiceFee,
                currency: @event.Currency,
                seatLabel: $"{sectionName} / General Admission",
                nowUtc: nowUtc);

            await _ticketDal.SaveNewTicketAsync(ticket, token);
            return ticket;
        }, ct);

    public async Task CancelAsync(
        long ticketId, int userId, string? reason, CancellationToken ct = default)
    {
        var ticket = await _ticketDal.GetWithEventAsync(ticketId, ct)
            ?? throw new EntityNotFoundException($"Ticket {ticketId} does not exist.");

        if (ticket.UserId != userId)
            throw new ForbiddenException($"Ticket {ticketId} belongs to another user.");

        ticket.Cancel(DateTime.UtcNow, ticket.Event.CancellationCutoffUtc, reason, userId);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Event> GetSellableEventAsync(int eventId, DateTime nowUtc, CancellationToken ct)
    {
        var @event = await _eventDal.GetAsync(e => e.Id == eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        if (!@event.IsOnSale(nowUtc))
            throw new BusinessRuleException($"Event {eventId} is not on sale.");

        return @event;
    }
}
