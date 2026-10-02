import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../events/event.service';
import { AlertifyService } from '../../shared/alertify.service';
import { AdminEvent, PriceInput, SimpleListItem } from '../../models';

interface PriceRow {
  seatCategoryId: number;
  categoryName: string;
  price: number;
  serviceFee: number;
}

@Component({
  selector: 'app-event-form',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-form.component.html'
})
export class EventFormComponent implements OnInit {
  private fb = inject(FormBuilder);

  eventId = signal<number | null>(null);
  venues = signal<SimpleListItem[]>([]);
  organizers = signal<SimpleListItem[]>([]);
  priceRows = signal<PriceRow[]>([]);
  currentEvent = signal<AdminEvent | null>(null);
  selectedPosterFile: File | null = null;

  form = this.fb.group({
    name: ['', Validators.required],
    venueId: [0, Validators.required],
    organizerId: [0, Validators.required],
    eventType: ['Concert', Validators.required],
    status: ['Draft'],
    currency: ['TRY', Validators.required],
    startsAtUtc: ['', Validators.required],
    endsAtUtc: ['', Validators.required],
    salesStartUtc: ['', Validators.required],
    salesEndUtc: ['', Validators.required],
    cancellationCutoffUtc: ['', Validators.required]
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private eventService: EventService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.eventService.getOrganizers().subscribe({
      next: (organizers) => this.organizers.set(organizers)
    });

    this.eventService.getVenues().subscribe({
      next: (venues) => this.venues.set(venues)
    });

    if (idParam) {
      const id = Number(idParam);
      this.eventId.set(id);
      this.loadForEdit(id);
    }
  }

  venueName(): string {
    const venueId = this.currentEvent()?.venueId;
    return this.venues().find((venue) => venue.id === venueId)?.name ?? '';
  }

  private loadForEdit(id: number) {
    this.eventService.getAdminDetail(id).subscribe({
      next: (event) => {
        this.currentEvent.set(event);
        this.form.patchValue({
          name: event.name,
          venueId: event.venueId,
          organizerId: event.organizerId,
          eventType: event.eventType,
          status: event.status,
          currency: event.currency,
          startsAtUtc: toLocalInput(event.startsAtUtc),
          endsAtUtc: toLocalInput(event.endsAtUtc),
          salesStartUtc: toLocalInput(event.salesStartUtc),
          salesEndUtc: toLocalInput(event.salesEndUtc),
          cancellationCutoffUtc: toLocalInput(event.cancellationCutoffUtc)
        });
        this.form.get('venueId')?.disable();

        this.eventService.getSeatCategories(event.venueId).subscribe({
          next: (categories) => this.loadPriceRows(categories, id)
        });
      },
      error: () => this.alertify.error('Etkinlik yüklenemedi')
    });
  }

  private loadPriceRows(categories: SimpleListItem[], id: number) {
    this.eventService.getById(id).subscribe({
      next: (detail) => {
        this.priceRows.set(
          categories.map((category) => {
            const existing = detail.prices.find((p) => p.seatCategoryId === category.id);
            return {
              seatCategoryId: category.id,
              categoryName: category.name,
              price: existing?.price ?? 0,
              serviceFee: existing?.serviceFee ?? 0
            };
          })
        );
      },
      error: () => {
        this.priceRows.set(
          categories.map((category) => ({
            seatCategoryId: category.id,
            categoryName: category.name,
            price: 0,
            serviceFee: 0
          }))
        );
      }
    });
  }

  updatePriceRow(index: number, field: 'price' | 'serviceFee', value: string) {
    const rows = [...this.priceRows()];
    rows[index] = { ...rows[index], [field]: Number(value) };
    this.priceRows.set(rows);
  }

  saveEvent() {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    const id = this.eventId();

    if (id === null) {
      const request = {
        name: value.name!,
        venueId: value.venueId!,
        organizerId: value.organizerId!,
        eventType: value.eventType!,
        currency: value.currency!,
        startsAtUtc: fromLocalInput(value.startsAtUtc!),
        endsAtUtc: fromLocalInput(value.endsAtUtc!),
        salesStartUtc: fromLocalInput(value.salesStartUtc!),
        salesEndUtc: fromLocalInput(value.salesEndUtc!),
        cancellationCutoffUtc: fromLocalInput(value.cancellationCutoffUtc!)
      };

      this.eventService.create(request).subscribe({
        next: (newId) => {
          this.alertify.success('Etkinlik oluşturuldu');
          this.router.navigate(['/admin/events', newId]);
        },
        error: (err) => this.alertify.error(err.error?.detail ?? 'Etkinlik oluşturulamadı')
      });
      return;
    }

    const updateRequest = {
      name: value.name!,
      eventType: value.eventType!,
      status: value.status!,
      startsAtUtc: fromLocalInput(value.startsAtUtc!),
      endsAtUtc: fromLocalInput(value.endsAtUtc!),
      salesStartUtc: fromLocalInput(value.salesStartUtc!),
      salesEndUtc: fromLocalInput(value.salesEndUtc!),
      cancellationCutoffUtc: fromLocalInput(value.cancellationCutoffUtc!)
    };

    this.eventService.update(id, updateRequest).subscribe({
      next: () => this.alertify.success('Etkinlik güncellendi'),
      error: (err) => this.alertify.error(err.error?.detail ?? 'Güncellenemedi')
    });
  }

  savePrices() {
    const id = this.eventId();
    if (id === null) {
      return;
    }

    const prices: PriceInput[] = this.priceRows().map((row) => ({
      seatCategoryId: row.seatCategoryId,
      price: row.price,
      serviceFee: row.serviceFee
    }));

    this.eventService.setPrices(id, prices).subscribe({
      next: () => this.alertify.success('Fiyatlar kaydedildi'),
      error: (err) => this.alertify.error(err.error?.detail ?? 'Fiyatlar kaydedilemedi')
    });
  }

  publish() {
    const id = this.eventId();
    if (id === null) {
      return;
    }

    this.alertify.confirm('Etkinlik yayınlansın mı? Koltuk envanteri oluşturulacak.', () => {
      this.eventService.publish(id).subscribe({
        next: () => {
          this.alertify.success('Etkinlik yayınlandı');
          this.loadForEdit(id);
        },
        error: (err) => this.alertify.error(err.error?.detail ?? 'Yayınlanamadı')
      });
    });
  }

  delete() {
    const id = this.eventId();
    if (id === null) {
      return;
    }

    this.alertify.confirm('Etkinlik silinsin mi? Bu işlem geri alınamaz.', () => {
      this.eventService.delete(id).subscribe({
        next: () => {
          this.alertify.success('Etkinlik silindi');
          this.router.navigate(['/events']);
        },
        error: (err) => this.alertify.error(err.error?.detail ?? 'Etkinlik silinemedi')
      });
    });
  }

  onPosterSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedPosterFile = input.files?.[0] ?? null;
  }

  uploadPoster() {
    const id = this.eventId();
    if (id === null || !this.selectedPosterFile) {
      return;
    }

    this.eventService.uploadPoster(id, this.selectedPosterFile).subscribe({
      next: (posterUrl) => {
        this.alertify.success('Afiş yüklendi');
        const current = this.currentEvent();
        if (current) {
          this.currentEvent.set({ ...current, posterUrl });
        }
      },
      error: (err) => this.alertify.error(err.error?.detail ?? 'Afiş yüklenemedi')
    });
  }
}

function toLocalInput(isoUtc: string): string {
  const date = new Date(isoUtc);
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60000);
  return local.toISOString().slice(0, 16);
}

function fromLocalInput(local: string): string {
  return new Date(local).toISOString();
}
