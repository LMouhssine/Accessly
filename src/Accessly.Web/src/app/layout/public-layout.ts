import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterLink, RouterOutlet, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary" class="topbar">
      <a routerLink="/" class="brand">Accessly</a>
      <nav class="nav">
        <a mat-button routerLink="/events">Events</a>
        @if (auth.isAuthenticated()) {
          <a mat-button routerLink="/my-tickets">My tickets</a>
          <a mat-button routerLink="/dashboard">Dashboard</a>
        } @else {
          <a mat-button routerLink="/login">Sign in</a>
        }
      </nav>
    </mat-toolbar>
    <main class="content"><router-outlet /></main>
  `,
  styles: [`
    .topbar { position: sticky; top: 0; z-index: 10; }
    .brand { font-weight: 700; font-size: 20px; text-decoration: none; color: inherit; margin-right: auto; }
    .nav { display: flex; gap: 4px; }
    .content { max-width: 1100px; margin: 0 auto; padding: 24px 16px; }
  `],
})
export class PublicLayoutComponent {
  readonly auth = inject(AuthService);
}
