import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/api.service';
import { EventSummary } from '../../core/models';
import { StatusBadgeComponent } from '../../shared/status-badge';

@Component({
  selector: 'app-events-admin',
  standalone: true,
  imports: [DatePipe, RouterLink, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatSnackBarModule, StatusBadgeComponent],
  template: `
    <div class="head">
      <h1>Events</h1>
      <a mat-flat-button color="primary" routerLink="/dashboard/events/new">New event</a>
    </div>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (events().length === 0) {
      <p class="muted">No events yet. Create your first event.</p>
    } @else {
      <mat-card appearance="outlined">
        @for (event of events(); track event.id) {
          <div class="row">
            <div class="info">
              <div class="title">{{ event.title }} <app-status-badge [status]="event.status" /></div>
              <div class="muted">{{ event.startAt | date: 'medium' }} · {{ event.bookedCount }}/{{ event.capacity }} booked</div>
            </div>
            <div class="actions">
              <a mat-button [routerLink]="['/dashboard/events', event.id]">Edit</a>
              @if (event.status === 'Draft') {
                <button mat-button color="primary" (click)="act(event, 'publish')">Publish</button>
              } @else if (event.status === 'Published') {
                <button mat-button (click)="act(event, 'unpublish')">Unpublish</button>
              }
              @if (event.status !== 'Cancelled' && event.status !== 'Completed') {
                <button mat-button color="warn" (click)="act(event, 'cancel')">Cancel</button>
              }
            </div>
          </div>
        }
      </mat-card>
    }
  `,
  styles: [`
    .head { display:flex; align-items:center; justify-content:space-between; }
    .center { display:flex; justify-content:center; padding:48px; }
    .row { display:flex; align-items:center; justify-content:space-between; padding:12px 16px; border-bottom:1px solid #eee; flex-wrap:wrap; gap:8px; }
    .row:last-child { border-bottom:none; }
    .title { font-weight:600; display:flex; align-items:center; gap:8px; }
    .muted { color:#5f6368; font-size:14px; }
  `],
})
export class EventsAdminComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);
  readonly events = signal<EventSummary[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getEvents({ publishedOnly: false, pageSize: 200 }).subscribe({
      next: (result) => {
        this.events.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  act(event: EventSummary, action: 'publish' | 'unpublish' | 'cancel'): void {
    const op =
      action === 'publish' ? this.api.publishEvent(event.id)
      : action === 'unpublish' ? this.api.unpublishEvent(event.id)
      : this.api.cancelEvent(event.id);

    op.subscribe({
      next: () => {
        this.snack.open(`Event ${action}ed.`, 'OK', { duration: 2500 });
        this.load();
      },
      error: (err) => this.snack.open(err?.error?.detail ?? 'Action failed.', 'OK', { duration: 3500 }),
    });
  }
}
