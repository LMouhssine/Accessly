import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../core/api.service';
import { EventSummary } from '../core/models';
import { EventCardComponent } from '../shared/event-card';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, EventCardComponent],
  template: `
    <h1>Upcoming events</h1>
    <mat-form-field appearance="outline" class="search">
      <mat-label>Search events</mat-label>
      <input matInput [(ngModel)]="search" (keyup.enter)="load()" placeholder="Title or keyword" />
    </mat-form-field>
    @if (loading()) {
      <div class="center"><mat-spinner diameter="40" /></div>
    } @else if (events().length === 0) {
      <p class="empty">No published events match your search.</p>
    } @else {
      <div class="grid">
        @for (event of events(); track event.id) {
          <app-event-card [event]="event" />
        }
      </div>
    }
  `,
  styles: [`
    .search { width:100%; max-width:420px; }
    .grid { display:grid; grid-template-columns:repeat(auto-fill, minmax(280px, 1fr)); gap:16px; }
    .center { display:flex; justify-content:center; padding:48px; }
    .empty { color:#5f6368; }
  `],
})
export class CatalogComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly events = signal<EventSummary[]>([]);
  readonly loading = signal(true);
  search = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getEvents({ search: this.search, pageSize: 24 }).subscribe({
      next: (result) => {
        this.events.set(result.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
