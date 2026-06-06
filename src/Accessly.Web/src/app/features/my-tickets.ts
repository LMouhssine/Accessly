import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../core/api.service';
import { Booking } from '../core/models';
import { StatusBadgeComponent } from '../shared/status-badge';
import { QrCodeDisplayComponent } from '../shared/qr-code-display';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatSnackBarModule, StatusBadgeComponent, QrCodeDisplayComponent],
  template: `
    <h1>My tickets</h1>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (bookings().length === 0) {
      <p class="muted">You have no bookings yet. Browse events to book a place.</p>
    } @else {
      <div class="grid">
        @for (booking of bookings(); track booking.id) {
          <mat-card appearance="outlined" class="ticket">
            <h3>{{ booking.eventTitle }}</h3>
            <p class="muted">{{ booking.eventStartAt | date: 'medium' }}</p>
            <p class="badges">
              <app-status-badge [status]="booking.status" />
              @if (booking.ticketStatus) { <app-status-badge [status]="booking.ticketStatus" /> }
            </p>
            @if (booking.status === 'Confirmed' && booking.ticketId) {
              <app-qr-code-display [ticketId]="booking.ticketId" />
              <p class="code">Code: <strong>{{ booking.ticketCode }}</strong></p>
              <button mat-stroked-button color="warn" (click)="cancel(booking)">Cancel booking</button>
            }
          </mat-card>
        }
      </div>
    }
  `,
  styles: [`
    .grid { display:grid; grid-template-columns:repeat(auto-fill, minmax(260px, 1fr)); gap:16px; }
    .ticket { padding:16px; text-align:center; }
    .ticket h3 { margin:0 0 4px; }
    .muted { color:#5f6368; }
    .badges { display:flex; gap:8px; justify-content:center; }
    .code { margin:8px 0; }
    .center { display:flex; justify-content:center; padding:48px; }
  `],
})
export class MyTicketsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);
  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getBookings({ pageSize: 50 }).subscribe({
      next: (result) => {
        this.bookings.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  cancel(booking: Booking): void {
    this.api.cancelBooking(booking.id).subscribe({
      next: () => {
        this.snack.open('Booking cancelled.', 'OK', { duration: 3000 });
        this.load();
      },
      error: () => this.snack.open('Could not cancel the booking.', 'OK', { duration: 3000 }),
    });
  }
}
