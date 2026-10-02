import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventService } from '../event.service';
import { EventDetail } from '../../models';
import { AlertifyService } from '../../shared/alertify.service';
import { AuthService } from '../../auth/auth.service';
import { SeatMapComponent } from '../../tickets/seat-map/seat-map.component';
import { OccupancyComponent } from '../../admin/occupancy/occupancy.component';

@Component({
  selector: 'app-event-detail',
  imports: [CommonModule, RouterLink, SeatMapComponent, OccupancyComponent],
  templateUrl: './event-detail.component.html'
})
export class EventDetailComponent implements OnInit {
  event = signal<EventDetail | null>(null);

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService,
    private alertify: AlertifyService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.eventService.getById(id).subscribe({
      next: (event) => this.event.set(event),
      error: () => this.alertify.error('Etkinlik bulunamadı')
    });
  }
}
