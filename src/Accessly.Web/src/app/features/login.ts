import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <div class="wrap">
      <mat-card class="card" appearance="outlined">
        <h1>Sign in to Accessly</h1>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline" class="full">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="username" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
          </mat-form-field>
          @if (error()) { <p class="err">{{ error() }}</p> }
          <button mat-flat-button color="primary" class="full" [disabled]="loading() || form.invalid">
            {{ loading() ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>
        <p class="hint">Demo accounts (password <code>Password123!</code>): admin@, organizer@, staff@, attendee@accessly.local</p>
      </mat-card>
    </div>
  `,
  styles: [`
    .wrap { display:flex; justify-content:center; padding:48px 16px; }
    .card { width:100%; max-width:420px; padding:24px; }
    .full { width:100%; }
    h1 { font-size:22px; margin:0 0 16px; }
    .err { color:#c5221f; margin:0 0 12px; }
    .hint { margin-top:16px; font-size:12px; color:#5f6368; }
  `],
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['organizer@accessly.local', [Validators.required, Validators.email]],
    password: ['Password123!', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.error.set('Invalid email or password.');
        this.loading.set(false);
      },
    });
  }
}
