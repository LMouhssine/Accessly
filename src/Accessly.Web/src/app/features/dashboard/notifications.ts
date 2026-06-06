import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/api.service';
import { NotificationItem } from '../../core/models';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatSnackBarModule],
  template: `
    <div class="head">
      <h1>Notifications</h1>
      <button mat-flat-button color="primary" (click)="sendTest()">Send test</button>
    </div>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (items().length === 0) {
      <p class="muted">No notifications yet.</p>
    } @else {
      <mat-card appearance="outlined">
        @for (item of items(); track item.id) {
          <div class="row">
            <div>
              <div class="subj">{{ item.subject }}</div>
              <div class="muted">{{ item.type }} · {{ item.toAddress }}</div>
            </div>
            <div class="muted">{{ item.createdAt | date: 'short' }}</div>
          </div>
        }
      </mat-card>
    }
  `,
  styles: [`
    .head { display:flex; justify-content:space-between; align-items:center; }
    .center { display:flex; justify-content:center; padding:48px; }
    .muted { color:#5f6368; font-size:14px; }
    .row { display:flex; justify-content:space-between; padding:12px 16px; border-bottom:1px solid #eee; }
    .row:last-child { border-bottom:none; }
    .subj { font-weight:600; }
  `],
})
export class NotificationsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);
  readonly items = signal<NotificationItem[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getNotifications().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  sendTest(): void {
    this.api.sendTestNotification().subscribe({
      next: () => {
        this.snack.open('Test notification sent.', 'OK', { duration: 2500 });
        this.load();
      },
      error: () => this.snack.open('Failed to send test notification.', 'OK', { duration: 2500 }),
    });
  }
}
