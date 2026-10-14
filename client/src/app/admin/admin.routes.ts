import { Routes } from '@angular/router';
import { AdminLayout } from './layout/admin-layout';
import {
  certificatesConfig,
  educationsConfig,
  experiencesConfig,
  projectsConfig,
  skillsConfig
} from './crud/crud-configs';

const crudPage = () => import('./crud/crud-page').then((m) => m.CrudPage);

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: '', loadComponent: () => import('./dashboard/dashboard').then((m) => m.DashboardPage), title: 'Panel · Admin' },
      { path: 'profile', loadComponent: () => import('./profile/profile').then((m) => m.ProfilePage), title: 'Profil · Admin' },
      // NOT: 5 sayfa da aynı CrudPage component'ini kullanıyor, sadece route data'daki config farklı
      { path: 'projects', loadComponent: crudPage, data: { config: projectsConfig }, title: 'Projeler · Admin' },
      { path: 'experiences', loadComponent: crudPage, data: { config: experiencesConfig }, title: 'Deneyimler · Admin' },
      { path: 'educations', loadComponent: crudPage, data: { config: educationsConfig }, title: 'Eğitim · Admin' },
      { path: 'skills', loadComponent: crudPage, data: { config: skillsConfig }, title: 'Yetenekler · Admin' },
      { path: 'certificates', loadComponent: crudPage, data: { config: certificatesConfig }, title: 'Sertifikalar · Admin' },
      { path: 'messages', loadComponent: () => import('./messages/messages').then((m) => m.MessagesPage), title: 'Mesajlar · Admin' },
      { path: 'settings', loadComponent: () => import('./settings/settings').then((m) => m.SettingsPage), title: 'Ayarlar · Admin' }
    ]
  }
];
