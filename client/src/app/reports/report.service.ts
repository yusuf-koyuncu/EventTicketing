import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EventOccupancy } from '../models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  constructor(private http: HttpClient) {}

  getOccupancy(eventId: number): Observable<EventOccupancy> {
    return this.http.get<EventOccupancy>(`/api/reports/events/${eventId}/occupancy`);
  }

  getRevenue(eventId: number): Observable<number> {
    return this.http.get<number>(`/api/reports/events/${eventId}/revenue`);
  }
}
