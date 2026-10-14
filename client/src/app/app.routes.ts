import { Routes } from '@angular/router';
import { adminGuard, guestGuard } from './core/auth/admin.guard';

// NOT: Tüm sayfalar loadComponent / loadChildren ile lazy load ediliyor.
// Ziyaretçi admin panelinin kodunu hiç indirmiyor, ilk açılış daha hızlı oluyor.
export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./public/layout/public-layout').then((m) => m.PublicLayout),
    children: [
      { path: '', loadComponent: () => import('./public/home/home').then((m) => m.Home) },
      { path: 'projects', loadComponent: () => import('./public/projects/projects-page').then((m) => m.ProjectsPage) },
      {
        path: 'projects/:slug',
        loadComponent: () => import('./public/projects/project-detail').then((m) => m.ProjectDetailPage)
      }
    ]
  },
  {
    path: 'admin/login',
    canActivate: [guestGuard],
    loadComponent: () => import('./admin/login/login').then((m) => m.Login)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadChildren: () => import('./admin/admin.routes').then((m) => m.adminRoutes)
  },
  {
    path: '**',
    loadComponent: () => import('./public/layout/public-layout').then((m) => m.PublicLayout),
    children: [{ path: '', loadComponent: () => import('./public/not-found').then((m) => m.NotFound) }]
  }
];
