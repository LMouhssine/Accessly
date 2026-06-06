import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <section class="hero">
      <h1>Run events. Welcome attendees.</h1>
      <p>Accessly helps organizers create events, manage bookings, issue QR code tickets and track live check-ins from a modern dashboard.</p>
      <div class="cta">
        <a mat-flat-button color="primary" routerLink="/events">Browse events</a>
        <a mat-stroked-button routerLink="/login">Organizer sign in</a>
      </div>
    </section>
    <section class="features">
      <div class="feature"><h3>Ticketing</h3><p>Bookings and QR code tickets with capacity guards.</p></div>
      <div class="feature"><h3>Live check-in</h3><p>Real-time attendance and a live feed at the door.</p></div>
      <div class="feature"><h3>Assistant</h3><p>Draft descriptions, agendas and summaries instantly.</p></div>
    </section>
  `,
  styles: [`
    .hero { text-align:center; padding:48px 0 32px; }
    .hero h1 { font-size:40px; margin:0 0 12px; }
    .hero p { max-width:620px; margin:0 auto 24px; color:#5f6368; font-size:18px; }
    .cta { display:flex; gap:12px; justify-content:center; }
    .features { display:grid; grid-template-columns:repeat(auto-fit,minmax(240px,1fr)); gap:16px; margin-top:32px; }
    .feature { background:#fff; border:1px solid #e0e0e0; border-radius:12px; padding:20px; }
    .feature h3 { margin:0 0 8px; }
    .feature p { margin:0; color:#5f6368; }
  `],
})
export class LandingComponent {}
