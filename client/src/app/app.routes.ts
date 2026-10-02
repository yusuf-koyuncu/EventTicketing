import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { adminGuard } from './admin/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'events',
    loadComponent: () =>
      import('./events/event-list/event-list.component').then((m) => m.EventListComponent)
  },
  {
    path: 'admin/events/new',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin/event-form/event-form.component').then((m) => m.EventFormComponent)
  },
  {
    path: 'admin/events/:id',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin/event-form/event-form.component').then((m) => m.EventFormComponent)
  },
  {
    path: 'events/:id',
    loadComponent: () =>
      import('./events/event-detail/event-detail.component').then((m) => m.EventDetailComponent)
  },
  {
    path: 'tickets/mine',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./tickets/my-tickets/my-tickets.component').then((m) => m.MyTicketsComponent)
  }
];
