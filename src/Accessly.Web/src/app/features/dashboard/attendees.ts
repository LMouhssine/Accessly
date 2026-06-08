import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';
import { Booking } from '../../core/models';

interface AttendeeRow {
  attendeeUserId: string;
  attendeeName: string;
  bookings: number;
  confirmed: number;
  lastEventTitle: string;
  lastBookingAt: string;
}

@Component({
  selector: 'app-attendees',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <h1>Attendees</h1>
    <p class="muted lead">People who have booked across your events, aggregated by attendee.</p>

    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (error()) {
      <mat-card appearance="outlined" class="state">
        <p>We couldn't load attendees.</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </mat-card>
    } @else if (rows().length === 0) {
      <mat-card appearance="outlined" class="state">
        <p class="muted">No attendees yet. They appear here once people book your events.</p>
      </mat-card>
    } @else {
      <mat-card appearance="outlined">
        <table class="grid">
          <thead>
            <tr><th>Attendee</th><th>Bookings</th><th>Confirmed</th><th>Most recent</th></tr>
          </thead>
          <tbody>
            @for (row of rows(); track row.attendeeUserId) {
              <tr>
                <td class="name">{{ row.attendeeName }}</td>
                <td>{{ row.bookings }}</td>
                <td>{{ row.confirmed }}</td>
                <td class="muted">{{ row.lastEventTitle }} · {{ row.lastBookingAt | date: 'mediumDate' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </mat-card>
    }
  `,
  styles: [`
    .lead { margin-top:-8px; }
    .center { display:flex; justify-content:center; padding:48px; }
    .state { padding:32px; text-align:center; }
    .muted { color:#5f6368; font-size:14px; }
    .grid { width:100%; border-collapse:collapse; }
    .grid th { text-align:left; font-size:12px; text-transform:uppercase; letter-spacing:.04em; color:#5f6368; padding:12px 16px; border-bottom:1px solid #eee; }
    .grid td { padding:12px 16px; border-bottom:1px solid #f1f1f1; font-size:14px; }
    .grid tr:last-child td { border-bottom:none; }
    .name { font-weight:600; }
  `],
})
export class AttendeesComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly rows = signal<AttendeeRow[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.api.getBookings({ pageSize: 200 }).subscribe({
      next: (result) => {
        this.rows.set(this.aggregate(result.items));
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  private aggregate(bookings: Booking[]): AttendeeRow[] {
    const byAttendee = new Map<string, AttendeeRow>();
    for (const booking of bookings) {
      const existing = byAttendee.get(booking.attendeeUserId);
      if (existing) {
        existing.bookings += 1;
        existing.confirmed += booking.status === 'Confirmed' ? 1 : 0;
        if (booking.createdAt > existing.lastBookingAt) {
          existing.lastBookingAt = booking.createdAt;
          existing.lastEventTitle = booking.eventTitle;
        }
      } else {
        byAttendee.set(booking.attendeeUserId, {
          attendeeUserId: booking.attendeeUserId,
          attendeeName: booking.attendeeName,
          bookings: 1,
          confirmed: booking.status === 'Confirmed' ? 1 : 0,
          lastEventTitle: booking.eventTitle,
          lastBookingAt: booking.createdAt,
        });
      }
    }

    return [...byAttendee.values()].sort((a, b) => b.lastBookingAt.localeCompare(a.lastBookingAt));
  }
}
