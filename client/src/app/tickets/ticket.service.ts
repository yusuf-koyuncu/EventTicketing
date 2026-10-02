import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Ticket, UserTicket } from '../models';

@Injectable({ providedIn: 'root' })
export class TicketService {
  constructor(private http: HttpClient) {}

  purchaseReservedSeat(eventId: number, eventSeatId: number): Observable<Ticket> {
    return this.http.post<Ticket>('/api/tickets/purchase/reserved-seat', { eventId, eventSeatId });
  }

  purchaseGeneralAdmission(eventId: number, eventSectionId: number): Observable<Ticket> {
    return this.http.post<Ticket>('/api/tickets/purchase/general-admission', {
      eventId,
      eventSectionId
    });
  }

  cancel(ticketId: number, reason?: string): Observable<void> {
    return this.http.post<void>(`/api/tickets/${ticketId}/cancel`, { reason });
  }

  getMine(): Observable<UserTicket[]> {
    return this.http.get<UserTicket[]>('/api/tickets/mine');
  }
}
