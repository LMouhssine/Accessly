import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="badge" [class]="'badge ' + tone()">{{ status }}</span>`,
  styles: [`
    .badge { display:inline-block; padding:2px 10px; border-radius:999px; font-size:12px; font-weight:600; line-height:18px; }
    .ok { background:#e6f4ea; color:#137333; }
    .warn { background:#fef7e0; color:#a06a00; }
    .bad { background:#fce8e6; color:#c5221f; }
    .muted { background:#e8eaed; color:#5f6368; }
  `],
})
export class StatusBadgeComponent {
  @Input() status = '';

  tone(): string {
    const value = this.status.toLowerCase();
    if (['published', 'confirmed', 'active', 'accepted', 'succeeded', 'completed', 'sent'].includes(value)) {
      return 'ok';
    }
    if (['draft', 'pending'].includes(value)) {
      return 'warn';
    }
    if (['cancelled', 'expired', 'failed', 'invalidticket', 'alreadycheckedin', 'eventmismatch', 'ticketcancelled', 'used'].includes(value)) {
      return 'bad';
    }
    return 'muted';
  }
}
