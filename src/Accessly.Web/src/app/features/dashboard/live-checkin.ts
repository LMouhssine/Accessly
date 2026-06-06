import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../core/api.service';
import { RealtimeService } from '../../core/realtime.service';
import { CheckIn, CheckInSummary, EventSummary } from '../../core/models';
import { StatusBadgeComponent } from '../../shared/status-badge';

@Component({
  selector: 'app-live-checkin',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, StatusBadgeComponent],
  template: `
    <h1>Live check-in</h1>
    <mat-form-field appearance="outline" class="picker">
      <mat-label>Event</mat-label>
      <mat-select [ngModel]="selectedId()" (ngModelChange)="onSelect($event)">
        @for (event of events(); track event.id) {
          <mat-option [value]="event.id">{{ event.title }}</mat-option>
        }
      </mat-select>
    </mat-form-field>

    @if (summary(); as s) {
      <div class="stats">
        <mat-card appearance="outlined" class="stat"><div class="num">{{ s.registered }}</div><div class="label">Registered</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ s.checkedIn }}</div><div class="label">Checked in</div></mat-card>
        <mat-card appearance="outlined" class="stat"><div class="num">{{ (s.fillRate * 100) | number: '1.0-0' }}%</div><div class="label">Fill rate</div></mat-card>
      </div>

      <mat-card appearance="outlined" class="scan">
        <mat-form-field appearance="outline" class="code">
          <mat-label>Ticket code</mat-label>
          <input matInput [(ngModel)]="code" (keyup.enter)="checkIn()" placeholder="e.g. ABCD234XYZ" />
        </mat-form-field>
        <button mat-flat-button color="primary" (click)="checkIn()">Check in</button>
        @if (lastMessage()) { <span class="msg">{{ lastMessage() }}</span> }
      </mat-card>

      <h2>Latest check-ins</h2>
      @if (feed().length === 0) {
        <p class="muted">No check-ins yet.</p>
      } @else {
        <mat-card appearance="outlined">
          @for (item of feed(); track item.id) {
            <div class="row">
              <span>{{ item.attendeeName }} · {{ item.ticketCode }}</span>
              <span><app-status-badge [status]="item.result" /> <span class="muted">{{ item.checkedInAt | date: 'mediumTime' }}</span></span>
            </div>
          }
        </mat-card>
      }
    } @else {
      <p class="muted">Select an event to start checking in.</p>
    }
  `,
  styles: [`
    .picker { width:100%; max-width:420px; }
    .stats { display:grid; grid-template-columns:repeat(3, minmax(120px, 1fr)); gap:16px; margin-bottom:16px; }
    .stat { padding:16px; text-align:center; } .num { font-size:32px; font-weight:700; } .label { color:#5f6368; }
    .scan { display:flex; align-items:center; gap:12px; padding:16px; flex-wrap:wrap; }
    .code { width:280px; }
    .msg { font-weight:600; }
    .row { display:flex; justify-content:space-between; padding:10px 16px; border-bottom:1px solid #eee; }
    .row:last-child { border-bottom:none; }
    .muted { color:#5f6368; }
  `],
})
export class LiveCheckinComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly realtime = inject(RealtimeService);

  readonly events = signal<EventSummary[]>([]);
  readonly selectedId = signal<string | null>(null);
  readonly summary = signal<CheckInSummary | null>(null);
  readonly feed = signal<CheckIn[]>([]);
  readonly lastMessage = signal<string | null>(null);
  code = '';

  ngOnInit(): void {
    this.api.getEvents({ publishedOnly: false, pageSize: 200 }).subscribe((result) =>
      this.events.set(result.items.filter((e) => e.status === 'Published')));
  }

  async onSelect(id: string): Promise<void> {
    this.selectedId.set(id);
    this.lastMessage.set(null);
    this.api.getCheckInSummary(id).subscribe((summary) => {
      this.summary.set(summary);
      this.feed.set(summary.recent);
    });
    await this.realtime.connect(id, (payload) => {
      this.summary.set(payload.summary);
      this.feed.set(payload.summary.recent);
    });
  }

  checkIn(): void {
    const id = this.selectedId();
    if (!id || !this.code.trim()) {
      return;
    }
    this.api.recordCheckIn(id, this.code.trim()).subscribe({
      next: (response) => {
        this.lastMessage.set(response.message);
        this.code = '';
      },
      error: () => this.lastMessage.set('Check-in failed.'),
    });
  }

  ngOnDestroy(): void {
    void this.realtime.disconnect();
  }
}
