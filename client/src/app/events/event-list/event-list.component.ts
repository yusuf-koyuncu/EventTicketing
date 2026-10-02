import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EventService } from '../event.service';
import { EventListItem, PagedResult } from '../../models';
import { AlertifyService } from '../../shared/alertify.service';

@Component({
  selector: 'app-event-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './event-list.component.html'
})
export class EventListComponent implements OnInit {
  result = signal<PagedResult<EventListItem> | null>(null);
  page = 1;
  pageSize = 12;

  constructor(private eventService: EventService, private alertify: AlertifyService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.eventService.getList(this.page, this.pageSize).subscribe({
      next: (result) => this.result.set(result),
      error: () => this.alertify.error('Etkinlikler yüklenemedi')
    });
  }

  nextPage() {
    if (this.result()?.hasNext) {
      this.page++;
      this.load();
    }
  }

  previousPage() {
    if (this.result()?.hasPrevious) {
      this.page--;
      this.load();
    }
  }
}
