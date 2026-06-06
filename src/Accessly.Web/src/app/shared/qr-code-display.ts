import { Component, Input, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ApiService } from '../core/api.service';

@Component({
  selector: 'app-qr-code-display',
  standalone: true,
  template: `
    @if (src()) {
      <img [src]="src()" alt="Ticket QR code" width="176" height="176" />
    } @else {
      <span class="muted">Loading QR…</span>
    }
  `,
  styles: [`.muted { color:#5f6368; font-size:13px; } img { border-radius:8px; }`],
})
export class QrCodeDisplayComponent implements OnInit, OnDestroy {
  @Input({ required: true }) ticketId!: string;

  private readonly api = inject(ApiService);
  private readonly sanitizer = inject(DomSanitizer);
  readonly src = signal<SafeUrl | null>(null);
  private objectUrl?: string;

  ngOnInit(): void {
    this.api.getTicketQr(this.ticketId).subscribe((blob) => {
      this.objectUrl = URL.createObjectURL(blob);
      this.src.set(this.sanitizer.bypassSecurityTrustUrl(this.objectUrl));
    });
  }

  ngOnDestroy(): void {
    if (this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
    }
  }
}
