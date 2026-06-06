import { Routes } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout';
import { DashboardLayoutComponent } from './layout/dashboard-layout';
import { authGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '', loadComponent: () => import('./features/landing').then((m) => m.LandingComponent) },
      { path: 'events', loadComponent: () => import('./features/catalog').then((m) => m.CatalogComponent) },
      { path: 'events/:id', loadComponent: () => import('./features/event-detail').then((m) => m.EventDetailComponent) },
      { path: 'my-tickets', canActivate: [authGuard], loadComponent: () => import('./features/my-tickets').then((m) => m.MyTicketsComponent) },
    ],
  },
  { path: 'login', loadComponent: () => import('./features/login').then((m) => m.LoginComponent) },
  {
    path: 'dashboard',
    component: DashboardLayoutComponent,
    canActivate: [roleGuard('Admin', 'Organizer', 'Staff')],
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/overview').then((m) => m.OverviewComponent) },
      { path: 'events', loadComponent: () => import('./features/dashboard/events-admin').then((m) => m.EventsAdminComponent) },
      { path: 'events/new', loadComponent: () => import('./features/dashboard/event-form').then((m) => m.EventFormComponent) },
      { path: 'events/:id', loadComponent: () => import('./features/dashboard/event-form').then((m) => m.EventFormComponent) },
      { path: 'check-in', loadComponent: () => import('./features/dashboard/live-checkin').then((m) => m.LiveCheckinComponent) },
      { path: 'bookings', loadComponent: () => import('./features/dashboard/bookings-admin').then((m) => m.BookingsAdminComponent) },
      { path: 'notifications', loadComponent: () => import('./features/dashboard/notifications').then((m) => m.NotificationsComponent) },
    ],
  },
  { path: '**', redirectTo: '' },
];
