import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../reports/report.service';
import { EventOccupancy } from '../../models';

@Component({
  selector: 'app-occupancy',
  imports: [CommonModule],
  templateUrl: './occupancy.component.html'
})
export class OccupancyComponent implements OnInit {
  @Input({ required: true }) eventId!: number;

  occupancy = signal<EventOccupancy | null>(null);
  revenue = signal<number | null>(null);

  constructor(private reportService: ReportService) {}

  ngOnInit() {
    this.reportService.getOccupancy(this.eventId).subscribe({
      next: (data) => this.occupancy.set(data)
    });

    this.reportService.getRevenue(this.eventId).subscribe({
      next: (data) => this.revenue.set(data)
    });
  }
}
