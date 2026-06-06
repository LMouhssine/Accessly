import { Component, Input } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { EventSummary } from '../core/models';
import { StatusBadgeComponent } from './status-badge';

@Component({
  selector: 'app-event-card',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, MatCardModule, MatButtonModule, StatusBadgeComponent],
  template: `
    <mat-card class="event-card" appearance="outlined">
      <mat-card-header>
        <mat-card-title>{{ event.title }}</mat-card-title>
        <mat-card-subtitle>{{ event.category || 'Event' }} · {{ event.venueName }}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <p class="when">{{ event.startAt | date: 'medium' }}</p>
        <p class="meta">
          <app-status-badge [status]="event.status" />
          <span>{{ event.bookedCount }} / {{ event.capacity }} booked</span>
          <span>{{ event.priceAmount === 0 ? 'Free' : (event.priceAmount | currency: event.currency) }}</span>
        </p>
      </mat-card-content>
      <mat-card-actions>
        <a mat-button color="primary" [routerLink]="['/events', event.id]">View details</a>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .event-card { height: 100%; }
    .when { color:#5f6368; margin: 0 0 8px; }
    .meta { display:flex; align-items:center; gap:12px; flex-wrap:wrap; font-size:14px; color:#3c4043; }
  `],
})
export class EventCardComponent {
  @Input({ required: true }) event!: EventSummary;
}
