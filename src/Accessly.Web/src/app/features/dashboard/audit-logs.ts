import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';
import { AuditLogEntry } from '../../core/models';

const ACTIONS = [
  'organization.created',
  'event.created',
  'event.updated',
  'event.published',
  'event.unpublished',
  'event.cancelled',
  'booking.created',
  'booking.cancelled',
  'checkin.recorded',
  'ai.generated',
];

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="head">
      <h1>Audit logs</h1>
      <mat-form-field appearance="outline" class="filter">
        <mat-label>Filter by action</mat-label>
        <mat-select [(ngModel)]="action" (selectionChange)="reload()">
          <mat-option [value]="''">All actions</mat-option>
          @for (a of actions; track a) {
            <mat-option [value]="a">{{ a }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (error()) {
      <mat-card appearance="outlined" class="state">
        <p>We couldn't load the audit log.</p>
        <button mat-stroked-button (click)="reload()">Try again</button>
      </mat-card>
    } @else if (items().length === 0) {
      <mat-card appearance="outlined" class="state">
        <p class="muted">No audit entries match this filter yet. Actions such as publishing an
        event or recording a check-in appear here.</p>
      </mat-card>
    } @else {
      <mat-card appearance="outlined">
        <table class="grid">
          <thead>
            <tr><th>When</th><th>Action</th><th>Entity</th><th>Actor</th></tr>
          </thead>
          <tbody>
            @for (entry of items(); track entry.id) {
              <tr>
                <td class="muted">{{ entry.timestamp | date: 'medium' }}</td>
                <td><span class="action">{{ entry.action }}</span></td>
                <td class="muted">{{ entry.entityType }}{{ entry.entityId ? ' · ' + shortId(entry.entityId) : '' }}</td>
                <td>{{ entry.actorDisplayName ?? 'System' }}</td>
              </tr>
            }
          </tbody>
        </table>
      </mat-card>

      <div class="pager">
        <button mat-stroked-button [disabled]="page() <= 1" (click)="go(page() - 1)">Previous</button>
        <span class="muted">Page {{ page() }} of {{ totalPages() }} · {{ total() }} entries</span>
        <button mat-stroked-button [disabled]="page() >= totalPages()" (click)="go(page() + 1)">Next</button>
      </div>
    }
  `,
  styles: [`
    .head { display:flex; justify-content:space-between; align-items:center; gap:16px; flex-wrap:wrap; }
    .filter { width:260px; }
    .center { display:flex; justify-content:center; padding:48px; }
    .state { padding:32px; text-align:center; }
    .muted { color:#5f6368; font-size:14px; }
    .grid { width:100%; border-collapse:collapse; }
    .grid th { text-align:left; font-size:12px; text-transform:uppercase; letter-spacing:.04em; color:#5f6368; padding:12px 16px; border-bottom:1px solid #eee; }
    .grid td { padding:12px 16px; border-bottom:1px solid #f1f1f1; font-size:14px; }
    .grid tr:last-child td { border-bottom:none; }
    .action { font-family:'SFMono-Regular', ui-monospace, monospace; font-size:13px; background:#f1f3f4; padding:2px 8px; border-radius:6px; }
    .pager { display:flex; align-items:center; justify-content:center; gap:16px; margin-top:16px; }
  `],
})
export class AuditLogsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly actions = ACTIONS;
  action = '';

  readonly items = signal<AuditLogEntry[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(1);
  readonly total = signal(0);
  readonly loading = signal(true);
  readonly error = signal(false);

  private readonly pageSize = 20;

  ngOnInit(): void {
    this.load();
  }

  reload(): void {
    this.page.set(1);
    this.load();
  }

  go(page: number): void {
    this.page.set(page);
    this.load();
  }

  shortId(id: string): string {
    return id.length > 8 ? `${id.slice(0, 8)}…` : id;
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.api
      .getAuditLogs({ page: this.page(), pageSize: this.pageSize, action: this.action || undefined })
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.total.set(result.totalCount);
          this.totalPages.set(Math.max(1, result.totalPages));
          this.loading.set(false);
        },
        error: () => {
          this.error.set(true);
          this.loading.set(false);
        },
      });
  }
}
