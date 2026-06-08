import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Organization } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [DatePipe, MatCardModule],
  template: `
    <h1>Settings</h1>
    <p class="muted lead">Your profile and workspace details.</p>

    <div class="cols">
      <mat-card appearance="outlined" class="panel">
        <h2>Profile</h2>
        <dl>
          <dt>Name</dt><dd>{{ auth.user()?.displayName }}</dd>
          <dt>Email</dt><dd>{{ auth.user()?.email }}</dd>
          <dt>Role</dt><dd>{{ auth.user()?.role }}</dd>
        </dl>
      </mat-card>

      <mat-card appearance="outlined" class="panel">
        <h2>Workspace</h2>
        @if (organization(); as org) {
          <dl>
            <dt>Organization</dt><dd>{{ org.name }}</dd>
            <dt>Slug</dt><dd>{{ org.slug }}</dd>
            <dt>Created</dt><dd>{{ org.createdAt | date: 'mediumDate' }}</dd>
          </dl>
        } @else {
          <p class="muted">No organization is associated with this account.</p>
        }
      </mat-card>
    </div>
  `,
  styles: [`
    .lead { margin-top:-8px; }
    .muted { color:#5f6368; font-size:14px; }
    .cols { display:grid; grid-template-columns:repeat(auto-fit, minmax(280px, 1fr)); gap:16px; }
    .panel { padding:20px; }
    .panel h2 { margin-top:0; font-size:16px; }
    dl { display:grid; grid-template-columns:auto 1fr; gap:8px 24px; margin:0; }
    dt { color:#5f6368; font-size:14px; }
    dd { margin:0; font-weight:500; }
  `],
})
export class SettingsComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  readonly organization = signal<Organization | null>(null);

  ngOnInit(): void {
    const organizationId = this.auth.user()?.organizationId;
    if (!organizationId) {
      return;
    }

    this.api.getOrganizations().subscribe({
      next: (orgs) => this.organization.set(orgs.find((o) => o.id === organizationId) ?? null),
    });
  }
}
