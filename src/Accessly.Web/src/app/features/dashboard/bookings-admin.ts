import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';
import { Booking } from '../../core/models';
import { StatusBadgeComponent } from '../../shared/status-badge';

@Component({
  selector: 'app-bookings-admin',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatProgressSpinnerModule, StatusBadgeComponent],
  template: `
    <h1>Bookings</h1>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (bookings().length === 0) {
      <p class="muted">No bookings yet.</p>
    } @else {
      <mat-card appearance="outlined">
        <table class="tbl">
          <thead><tr><th>Event</th><th>Attendee</th><th>Status</th><th>Ticket</th><th>Booked</th></tr></thead>
          <tbody>
            @for (booking of bookings(); track booking.id) {
              <tr>
                <td>{{ booking.eventTitle }}</td>
                <td>{{ booking.attendeeName }}</td>
                <td><app-status-badge [status]="booking.status" /></td>
                <td>{{ booking.ticketCode || '—' }}</td>
                <td class="muted">{{ booking.createdAt | date: 'short' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </mat-card>
    }
  `,
  styles: [`
    .center { display:flex; justify-content:center; padding:48px; }
    .muted { color:#5f6368; }
    .tbl { width:100%; border-collapse:collapse; }
    .tbl th, .tbl td { text-align:left; padding:10px 12px; border-bottom:1px solid #eee; font-size:14px; }
    .tbl th { color:#5f6368; font-weight:600; }
  `],
})
export class BookingsAdminComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api.getBookings({ pageSize: 200 }).subscribe({
      next: (result) => {
        this.bookings.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
