import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/api.service';
import { EventSummary, FeedbackList } from '../../core/models';

@Component({
  selector: 'app-feedbacks',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  template: `
    <h1>Feedback</h1>
    <p class="muted lead">Attendee ratings and comments per event, with an optional generated summary.</p>

    <mat-form-field appearance="outline" class="picker">
      <mat-label>Event</mat-label>
      <mat-select [(ngModel)]="eventId" (selectionChange)="loadFeedback()">
        @for (event of events(); track event.id) {
          <mat-option [value]="event.id">{{ event.title }}</mat-option>
        }
      </mat-select>
    </mat-form-field>

    @if (!eventId) {
      <mat-card appearance="outlined" class="state">
        <p class="muted">Select an event to review its feedback.</p>
      </mat-card>
    } @else if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (feedback(); as list) {
      <div class="stats">
        <mat-card appearance="outlined" class="stat">
          <div class="num">{{ list.averageRating | number: '1.1-1' }}</div>
          <div class="label">Average rating</div>
        </mat-card>
        <mat-card appearance="outlined" class="stat">
          <div class="num">{{ list.count }}</div>
          <div class="label">Responses</div>
        </mat-card>
      </div>

      <div class="ai-bar">
        <button mat-stroked-button [disabled]="list.count === 0 || summarizing()" (click)="summarize()">
          {{ summarizing() ? 'Summarizing…' : 'Generate summary' }}
        </button>
      </div>
      @if (summary()) {
        <mat-card appearance="outlined" class="summary">
          <div class="label">Summary</div>
          <p>{{ summary() }}</p>
        </mat-card>
      }

      @if (list.items.length === 0) {
        <mat-card appearance="outlined" class="state">
          <p class="muted">No feedback for this event yet.</p>
        </mat-card>
      } @else {
        <mat-card appearance="outlined">
          @for (item of list.items; track item.id) {
            <div class="row">
              <div>
                <div class="rating">{{ stars(item.rating) }}</div>
                @if (item.comment) { <div class="comment">{{ item.comment }}</div> }
              </div>
              <div class="muted">{{ item.createdAt | date: 'mediumDate' }}</div>
            </div>
          }
        </mat-card>
      }
    }
  `,
  styles: [`
    .lead { margin-top:-8px; }
    .picker { width:320px; }
    .center { display:flex; justify-content:center; padding:48px; }
    .state { padding:32px; text-align:center; }
    .muted { color:#5f6368; font-size:14px; }
    .stats { display:grid; grid-template-columns:repeat(auto-fit, minmax(160px, 1fr)); gap:16px; margin-bottom:16px; }
    .stat { padding:20px; text-align:center; }
    .num { font-size:32px; font-weight:700; }
    .label { color:#5f6368; font-size:13px; text-transform:uppercase; letter-spacing:.04em; }
    .ai-bar { margin-bottom:16px; }
    .summary { padding:16px; margin-bottom:16px; }
    .summary p { margin:8px 0 0; }
    .row { display:flex; justify-content:space-between; gap:16px; padding:12px 16px; border-bottom:1px solid #f1f1f1; }
    .row:last-child { border-bottom:none; }
    .rating { color:#f5a623; letter-spacing:2px; }
    .comment { margin-top:4px; }
  `],
})
export class FeedbacksComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);

  readonly events = signal<EventSummary[]>([]);
  readonly feedback = signal<FeedbackList | null>(null);
  readonly summary = signal('');
  readonly loading = signal(false);
  readonly summarizing = signal(false);
  eventId = '';

  ngOnInit(): void {
    this.api.getEvents({ publishedOnly: false, pageSize: 200 }).subscribe({
      next: (result) => this.events.set(result.items),
    });
  }

  loadFeedback(): void {
    if (!this.eventId) {
      return;
    }

    this.summary.set('');
    this.loading.set(true);
    this.api.getFeedbacks(this.eventId).subscribe({
      next: (list) => {
        this.feedback.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.feedback.set(null);
        this.loading.set(false);
        this.snack.open('Failed to load feedback.', 'OK', { duration: 2500 });
      },
    });
  }

  summarize(): void {
    if (!this.eventId) {
      return;
    }

    this.summarizing.set(true);
    this.api.summarizeFeedback(this.eventId).subscribe({
      next: (response) => {
        this.summary.set(response.text);
        this.summarizing.set(false);
      },
      error: () => {
        this.summarizing.set(false);
        this.snack.open('Failed to generate summary.', 'OK', { duration: 2500 });
      },
    });
  }

  stars(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(Math.max(0, 5 - rating));
  }
}
