import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService, EventInput } from '../../core/api.service';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSnackBarModule],
  template: `
    <h1>{{ id ? 'Edit event' : 'New event' }}</h1>
    <mat-card appearance="outlined" class="card">
      <form [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Title</mat-label>
          <input matInput formControlName="title" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Category</mat-label>
          <input matInput formControlName="category" />
        </mat-form-field>

        <div class="desc-head">
          <span>Description</span>
          <button type="button" mat-button color="primary" [disabled]="generating()" (click)="generateDescription()">
            {{ generating() ? 'Generating…' : 'Generate with assistant' }}
          </button>
        </div>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Description</mat-label>
          <textarea matInput rows="4" formControlName="description"></textarea>
        </mat-form-field>

        <div class="grid">
          <mat-form-field appearance="outline">
            <mat-label>Starts at</mat-label>
            <input matInput type="datetime-local" formControlName="startAt" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Ends at</mat-label>
            <input matInput type="datetime-local" formControlName="endAt" />
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline" class="full">
          <mat-label>Venue name</mat-label>
          <input matInput formControlName="venueName" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Venue address</mat-label>
          <input matInput formControlName="venueAddress" />
        </mat-form-field>

        <div class="grid">
          <mat-form-field appearance="outline">
            <mat-label>Capacity</mat-label>
            <input matInput type="number" formControlName="capacity" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Price</mat-label>
            <input matInput type="number" formControlName="priceAmount" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Currency</mat-label>
            <input matInput formControlName="currency" maxlength="3" />
          </mat-form-field>
        </div>

        <div class="actions">
          <button mat-flat-button color="primary" [disabled]="saving() || form.invalid">
            {{ saving() ? 'Saving…' : (id ? 'Save changes' : 'Create event') }}
          </button>
        </div>
      </form>
    </mat-card>
  `,
  styles: [`
    .card { max-width:720px; padding:24px; }
    .full { width:100%; }
    .grid { display:grid; grid-template-columns:repeat(auto-fit, minmax(160px, 1fr)); gap:12px; }
    .desc-head { display:flex; justify-content:space-between; align-items:center; margin-bottom:4px; color:#5f6368; }
    .actions { margin-top:8px; }
  `],
})
export class EventFormComponent implements OnInit {
  @Input() id?: string;

  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  readonly saving = signal(false);
  readonly generating = signal(false);

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    category: [''],
    description: ['', Validators.required],
    startAt: ['', Validators.required],
    endAt: ['', Validators.required],
    venueName: ['', Validators.required],
    venueAddress: [''],
    capacity: [100, [Validators.required, Validators.min(1)]],
    priceAmount: [0, [Validators.required, Validators.min(0)]],
    currency: ['EUR', [Validators.required, Validators.maxLength(3)]],
  });

  ngOnInit(): void {
    if (this.id) {
      this.api.getEvent(this.id).subscribe((event) => {
        this.form.patchValue({
          title: event.title,
          category: event.category ?? '',
          description: event.description,
          startAt: this.toLocalInput(event.startAt),
          endAt: this.toLocalInput(event.endAt),
          venueName: event.venueName,
          venueAddress: event.venueAddress ?? '',
          capacity: event.capacity,
          priceAmount: event.priceAmount,
          currency: event.currency,
        });
      });
    }
  }

  generateDescription(): void {
    const value = this.form.getRawValue();
    if (!value.title) {
      this.snack.open('Enter a title first.', 'OK', { duration: 2500 });
      return;
    }
    this.generating.set(true);
    this.api.aiEventDescription({ title: value.title, category: value.category }).subscribe({
      next: (result) => {
        this.form.patchValue({ description: result.text });
        this.generating.set(false);
      },
      error: () => this.generating.set(false),
    });
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }
    this.saving.set(true);
    const value = this.form.getRawValue();
    const input: EventInput = {
      ...value,
      startAt: new Date(value.startAt).toISOString(),
      endAt: new Date(value.endAt).toISOString(),
    };

    const operation: Observable<unknown> = this.id ? this.api.updateEvent(this.id, input) : this.api.createEvent(input);
    operation.subscribe({
      next: () => {
        this.snack.open(this.id ? 'Event updated.' : 'Event created.', 'OK', { duration: 2500 });
        this.router.navigate(['/dashboard/events']);
      },
      error: (err) => {
        this.snack.open(err?.error?.detail ?? 'Could not save the event.', 'OK', { duration: 3500 });
        this.saving.set(false);
      },
    });
  }

  private toLocalInput(iso: string): string {
    const date = new Date(iso);
    const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 16);
  }
}
