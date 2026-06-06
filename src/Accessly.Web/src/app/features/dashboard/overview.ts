import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';
import { EventSummary } from '../../core/models';

@Component({
  selector: 'app-overview',
  standalone: true,
  imports: [DatePipe, RouterLink, MatCardModule, MatProgressSpinnerModule],
  template: `
    <h1>Overview</h1>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else {
      <div class="stats">
        <mat-card appearance="outlined" class="stat"><div class="num">{{ total() }}</div><div class="label">Total events</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ published() }}</div><div class="label">Published</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ draft() }}</div><div class="label">Drafts</div></mat-card>
      </div>
      <h2>Upcoming published events</h2>
      @if (upcoming().length === 0) {
        <p class="muted">No published events yet.</p>
      } @else {
        <mat-card appearance="outlined">
          @for (event of upcoming(); track event.id) {
            <a class="row" [routerLink]="['/dashboard/events']">
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
  readonly total = signal(0);
  readonly published = signal(0);
  readonly draft = signal(0);
  readonly upcoming = signal<EventSummary[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api.getEvents({ publishedOnly: false, pageSize: 200 }).subscribe({
      next: (result) => {
        const items = result.items;
        this.total.set(result.totalCount);
        this.published.set(items.filter((e) => e.status === 'Published').length);
        this.draft.set(items.filter((e) => e.status === 'Draft').length);
        this.upcoming.set(items.filter((e) => e.status === 'Published').slice(0, 5));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
