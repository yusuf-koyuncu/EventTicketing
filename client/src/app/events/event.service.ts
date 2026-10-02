import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdminEvent,
  AvailableSeat,
  CreateEventRequest,
  EventDetail,
  EventListItem,
  PagedResult,
  PriceInput,
  SectionAvailability,
  SimpleListItem,
  UpdateEventRequest
} from '../models';

@Injectable({ providedIn: 'root' })
export class EventService {
  constructor(private http: HttpClient) {}

  getList(page: number, pageSize: number): Observable<PagedResult<EventListItem>> {
    return this.http.get<PagedResult<EventListItem>>('/api/events', {
      params: { page, pageSize }
    });
  }

  getById(id: number): Observable<EventDetail> {
    return this.http.get<EventDetail>(`/api/events/${id}`);
  }

  getSections(id: number): Observable<SectionAvailability[]> {
    return this.http.get<SectionAvailability[]>(`/api/events/${id}/sections`);
  }

  getAvailableSeats(id: number, eventSectionId?: number): Observable<AvailableSeat[]> {
    return this.http.get<AvailableSeat[]>(`/api/events/${id}/seats`, {
      params: eventSectionId ? { eventSectionId } : {}
    });
  }

  getVenues(): Observable<SimpleListItem[]> {
    return this.http.get<SimpleListItem[]>('/api/events/venues');
  }

  getOrganizers(): Observable<SimpleListItem[]> {
    return this.http.get<SimpleListItem[]>('/api/events/organizers');
  }

  getSeatCategories(venueId: number): Observable<SimpleListItem[]> {
    return this.http.get<SimpleListItem[]>(`/api/events/venues/${venueId}/seat-categories`);
  }

  getAdminDetail(id: number): Observable<AdminEvent> {
    return this.http.get<AdminEvent>(`/api/events/${id}/admin`);
  }

  create(request: CreateEventRequest): Observable<number> {
    return this.http.post<number>('/api/events', request);
  }

  update(id: number, request: UpdateEventRequest): Observable<void> {
    return this.http.put<void>(`/api/events/${id}`, request);
  }

  setPrices(id: number, prices: PriceInput[]): Observable<void> {
    return this.http.put<void>(`/api/events/${id}/prices`, { prices });
  }

  publish(id: number): Observable<void> {
    return this.http.post<void>(`/api/events/${id}/publish`, {});
  }

  uploadPoster(id: number, file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<string>(`/api/events/${id}/poster`, formData);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/events/${id}`);
  }
}
