import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EventService } from '../../events/event.service';
import { TicketService } from '../ticket.service';
import { AuthService } from '../../auth/auth.service';
import { AlertifyService } from '../../shared/alertify.service';
import { AvailableSeat, SectionAvailability } from '../../models';

@Component({
  selector: 'app-seat-map',
  imports: [CommonModule],
  templateUrl: './seat-map.component.html'
})
export class SeatMapComponent implements OnInit {
  @Input({ required: true }) eventId!: number;

  sections = signal<SectionAvailability[]>([]);
  expandedSectionId = signal<number | null>(null);
  seats = signal<AvailableSeat[]>([]);
  busy = signal(false);

  constructor(
    private eventService: EventService,
    private ticketService: TicketService,
    private authService: AuthService,
    private alertify: AlertifyService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadSections();
  }

  loadSections() {
    this.eventService.getSections(this.eventId).subscribe({
      next: (sections) => this.sections.set(sections),
      error: () => this.alertify.error('Bölümler yüklenemedi')
    });
  }

  remaining(section: SectionAvailability): number {
    return Math.max(0, section.capacity - section.sold);
  }

  toggleSection(section: SectionAvailability) {
    if (this.expandedSectionId() === section.eventSectionId) {
      this.expandedSectionId.set(null);
      this.seats.set([]);
      return;
    }

    this.expandedSectionId.set(section.eventSectionId);
    this.eventService.getAvailableSeats(this.eventId, section.eventSectionId).subscribe({
      next: (seats) => this.seats.set(seats),
      error: () => this.alertify.error('Koltuklar yüklenemedi')
    });
  }

  private requireLogin(): boolean {
    if (this.authService.isLoggedIn()) {
      return true;
    }

    this.alertify.error('Bilet almak için giriş yapmalısınız');
    this.router.navigate(['/login']);
    return false;
  }

  buySeat(seat: AvailableSeat) {
    if (!this.requireLogin()) {
      return;
    }

    const label = seat.rowLabel
      ? `${seat.sectionName} / Sıra ${seat.rowLabel} / Koltuk ${seat.seatNumber}`
      : `${seat.sectionName} / Koltuk ${seat.seatNumber}`;

    this.alertify.confirm(`${label} — ${seat.price} TRY karşılığında satın alınsın mı?`, () => {
      this.busy.set(true);
      this.ticketService.purchaseReservedSeat(this.eventId, seat.eventSeatId).subscribe({
        next: () => {
          this.alertify.success('Bilet satın alındı');
          this.busy.set(false);
          this.loadSections();
          const sectionId = this.expandedSectionId();
          if (sectionId !== null) {
            this.eventService.getAvailableSeats(this.eventId, sectionId).subscribe({
              next: (seats) => this.seats.set(seats)
            });
          }
        },
        error: (err) => {
          this.busy.set(false);
          this.alertify.error(err.error?.detail ?? 'Koltuk satın alınamadı');
          this.loadSections();
        }
      });
    });
  }

  buyGeneralAdmission(section: SectionAvailability) {
    if (!this.requireLogin()) {
      return;
    }

    this.alertify.confirm(`${section.sectionName} için genel giriş bileti alınsın mı?`, () => {
      this.busy.set(true);
      this.ticketService.purchaseGeneralAdmission(this.eventId, section.eventSectionId).subscribe({
        next: () => {
          this.alertify.success('Bilet satın alındı');
          this.busy.set(false);
          this.loadSections();
        },
        error: (err) => {
          this.busy.set(false);
          this.alertify.error(err.error?.detail ?? 'Bilet satın alınamadı');
          this.loadSections();
        }
      });
    });
  }
}
