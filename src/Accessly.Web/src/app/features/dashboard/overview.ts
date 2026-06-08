import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';
import { DashboardSummary } from '../../core/models';

@Component({
  selector: 'app-overview',
  standalone: true,
  imports: [DatePipe, RouterLink, MatCardModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <h1>Overview</h1>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (error()) {
      <mat-card appearance="outlined" class="state">
        <p>We couldn't load the dashboard.</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </mat-card>
    } @else if (summary(); as data) {
      <div class="stats">
        <mat-card appearance="outlined" class="stat"><div class="num">{{ data.totalEvents }}</div><div class="label">Total events</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ data.publishedEvents }}</div><div class="label">Published</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ data.draftEvents }}</div><div class="label">Drafts</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ data.confirmedBookings }}</div><div class="label">Confirmed bookings</div></mat-card>
      </div>
      <h2>Upcoming published events</h2>
      @if (data.upcoming.length === 0) {
        <mat-card appearance="outlined" class="state">
          <p class="muted">No upcoming published events. Create one from the Events page to get started.</p>
          <button mat-flat-button color="primary" routerLink="/dashboard/events/new">New event</button>
        </mat-card>
      } @else {
        <mat-card appearance="outlined">
          @for (event of data.upcoming; track event.id) {
            <a class="row" [routerLink]="['/dashboard/events', event.id]">
              <span>{{ event.title }}</span>
              <span class="muted">{{ event.startAt | date: 'medium' }} · {{ event.bookedCount }}/{{ event.capacity }}</span>
            </a>
          }
        </mat-card>
      }
    }
  `,
  styles: [`
    .center { display:flex; justify-content:center; padding:48px; }
    .state { padding:32px; text-align:center; }
    .stats { display:grid; grid-template-columns:repeat(auto-fit, minmax(180px, 1fr)); gap:16px; margin-bottom:24px; }
    .stat { padding:20px; text-align:center; }
    .num { font-size:36px; font-weight:700; }
    .label { color:#5f6368; }
    .row { display:flex; justify-content:space-between; padding:12px 16px; border-bottom:1px solid #eee; text-decoration:none; color:inherit; }
    .row:last-child { border-bottom:none; }
    .muted { color:#5f6368; }
  `],
})
export class OverviewComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.api.getDashboardSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }
}
