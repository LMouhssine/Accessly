import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, MatToolbarModule, MatSidenavModule, MatListModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary" class="topbar">
      <a routerLink="/" class="brand">Accessly</a>
      <span class="spacer"></span>
      <span class="user">{{ auth.user()?.displayName }} · {{ auth.user()?.role }}</span>
      <button mat-button (click)="logout()">Sign out</button>
    </mat-toolbar>
    <mat-sidenav-container class="shell">
      <mat-sidenav mode="side" opened class="side">
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Overview</a>
          <a mat-list-item routerLink="/dashboard/events" routerLinkActive="active">Events</a>
          <a mat-list-item routerLink="/dashboard/check-in" routerLinkActive="active">Live check-in</a>
          <a mat-list-item routerLink="/dashboard/bookings" routerLinkActive="active">Bookings</a>
          <a mat-list-item routerLink="/dashboard/notifications" routerLinkActive="active">Notifications</a>
        </mat-nav-list>
      </mat-sidenav>
      <mat-sidenav-content class="main"><router-outlet /></mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .topbar { position: sticky; top: 0; z-index: 10; }
    .brand { font-weight: 700; font-size: 20px; text-decoration: none; color: inherit; }
    .spacer { flex: 1 1 auto; }
    .user { margin-right: 8px; font-size: 14px; opacity: 0.9; }
    .shell { height: calc(100vh - 64px); }
    .side { width: 230px; padding-top: 8px; }
    .main { padding: 24px; background: #fafafa; }
    .active { background: rgba(0, 0, 0, 0.06); }
  `],
})
export class DashboardLayoutComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
