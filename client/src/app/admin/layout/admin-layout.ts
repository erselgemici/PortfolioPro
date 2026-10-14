import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ThemeService } from '../../core/theme.service';
import { Icon } from '../../shared/icon';
import { AdminState } from '../admin-state.service';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  exact?: boolean;
  badge?: boolean;
}

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Icon],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminLayout {
  protected readonly auth = inject(AuthService);
  protected readonly theme = inject(ThemeService);
  protected readonly state = inject(AdminState);

  protected readonly sidebarOpen = signal(false);

  protected readonly nav: NavItem[] = [
    { path: '/admin', label: 'Panel', icon: 'dashboard', exact: true },
    { path: '/admin/profile', label: 'Profil', icon: 'user' },
    { path: '/admin/projects', label: 'Projeler', icon: 'folder' },
    { path: '/admin/experiences', label: 'Deneyimler', icon: 'briefcase' },
    { path: '/admin/educations', label: 'Eğitim', icon: 'graduation' },
    { path: '/admin/skills', label: 'Yetenekler', icon: 'layers' },
    { path: '/admin/certificates', label: 'Sertifikalar', icon: 'award' },
    { path: '/admin/messages', label: 'Mesajlar', icon: 'inbox', badge: true },
    { path: '/admin/settings', label: 'Ayarlar', icon: 'sliders' }
  ];

  constructor() {
    this.state.refreshUnread();

    // Mobilde bir sayfaya geçince menüyü kapat
    inject(Router)
      .events.pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe(() => this.sidebarOpen.set(false));
  }
}
