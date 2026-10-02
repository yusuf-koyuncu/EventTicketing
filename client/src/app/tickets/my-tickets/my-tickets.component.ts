import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TicketService } from '../ticket.service';
import { UserTicket } from '../../models';
import { AlertifyService } from '../../shared/alertify.service';

@Component({
  selector: 'app-my-tickets',
  imports: [CommonModule],
  templateUrl: './my-tickets.component.html'
})
export class MyTicketsComponent implements OnInit {
  tickets = signal<UserTicket[]>([]);

  constructor(private ticketService: TicketService, private alertify: AlertifyService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.ticketService.getMine().subscribe({
      next: (tickets) => this.tickets.set(tickets),
      error: () => this.alertify.error('Biletler yüklenemedi')
    });
  }

  cancel(ticket: UserTicket) {
    this.alertify.confirm(`${ticket.eventName} için bileti iptal etmek istiyor musunuz?`, () => {
      this.ticketService.cancel(ticket.ticketId, 'Kullanıcı isteği').subscribe({
        next: () => {
          this.alertify.success('Bilet iptal edildi');
          this.load();
        },
        error: (err) => this.alertify.error(err.error?.detail ?? 'Bilet iptal edilemedi')
      });
    });
  }
}
