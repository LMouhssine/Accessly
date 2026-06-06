import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { EventDetail } from '../core/models';
import { StatusBadgeComponent } from '../shared/status-badge';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatSnackBarModule, StatusBadgeComponent],
  template: `
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (event(); as e) {
      <a routerLink="/events" class="back">← Back to events</a>
      <mat-card appearance="outlined" class="detail">
        <div class="head">
          <h1>{{ e.title }}</h1>
          <app-status-badge [status]="e.status" />
        </div>
        <p class="muted">{{ e.startAt | date: 'full' }} · {{ e.venueName }}@if (e.venueAddress) {, {{ e.venueAddress }}}</p>
        <p class="price">{{ e.priceAmount === 0 ? 'Free' : (e.priceAmount | currency: e.currency) }} · {{ e.bookedCount }} / {{ e.capacity }} booked</p>
        <p class="desc">{{ e.description }}</p>
        @if (e.speakers.length) {
          <h3>Speakers</h3>
          <ul>
            @for (speaker of e.speakers; track speaker.id) {
              <li><strong>{{ speaker.name }}</strong>@if (speaker.title) { — {{ speaker.title }}}</li>
            }
          </ul>
        }
        <div class="actions">
          @if (isAuthenticated()) {
            <button mat-flat-button color="primary" [disabled]="booking() || isFull() || e.status !== 'Published'" (click)="book()">
              {{ isFull() ? 'Sold out' : (booking() ? 'Booking…' : 'Book a place') }}
            </button>
          } @else {
            <a mat-flat-button color="primary" routerLink="/login">Sign in to book</a>
          }
        </div>
      </mat-card>
    } @else {
      <p>Event not found.</p>
    }
  `,
  styles: [`
    .center { display:flex; justify-content:center; padding:48px; }
    .back { display:inline-block; margin-bottom:12px; }
    .detail { padding:24px; }
    .head { display:flex; align-items:center; gap:12px; }
    .head h1 { margin:0; font-size:28px; }
    .muted { color:#5f6368; }
    .price { font-weight:600; }
    .desc { white-space:pre-line; line-height:1.6; }
    .actions { margin-top:16px; }
  `],
})
export class EventDetailComponent implements OnInit {
  @Input() id!: string;

  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly snack = inject(MatSnackBar);
  private readonly router = inject(Router);

  readonly event = signal<EventDetail | null>(null);
  readonly loading = signal(true);
  readonly booking = signal(false);
  readonly isAuthenticated = this.auth.isAuthenticated;

  isFull(): boolean {
    const current = this.event();
    return current !== null && current.bookedCount >= current.capacity;
  }

  ngOnInit(): void {
    this.api.getEvent(this.id).subscribe({
      next: (event) => {
        this.event.set(event);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  book(): void {
    this.booking.set(true);
    this.api.createBooking(this.id).subscribe({
      next: () => {
        this.snack.open('Booking confirmed — your ticket has been issued.', 'OK', { duration: 3500 });
        this.router.navigate(['/my-tickets']);
      },
      error: (err) => {
        this.snack.open(err?.error?.detail ?? 'Unable to complete the booking.', 'OK', { duration: 4000 });
        this.booking.set(false);
      },
    });
  }
}
