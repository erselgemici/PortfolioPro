import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { ThemeService } from '../../core/theme.service';
import { I18nService } from '../../core/i18n/i18n.service';
import { TranslatePipe } from '../../core/i18n/i18n.pipes';
import { TranslationKey } from '../../core/i18n/translations';
import { PortfolioApi } from '../../core/api/portfolio-api.service';
import { Icon } from '../../shared/icon';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, Icon, TranslatePipe],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PublicLayout {
  protected readonly theme = inject(ThemeService);
  protected readonly i18n = inject(I18nService);
  private readonly router = inject(Router);

  // Footer'daki isim ve sosyal linkler için profil (cache'li, ekstra istek atmıyor)
  protected readonly portfolio = toSignal(inject(PortfolioApi).getPortfolio(), { initialValue: null });

  protected readonly year = new Date().getFullYear();
  protected readonly menuOpen = signal(false);
  protected readonly scrolled = signal(false);

  protected readonly links: { fragment: string; key: TranslationKey }[] = [
    { fragment: 'about', key: 'nav.about' },
    { fragment: 'skills', key: 'nav.skills' },
    { fragment: 'experience', key: 'nav.experience' },
    { fragment: 'projects', key: 'nav.projects' },
    { fragment: 'contact', key: 'nav.contact' }
  ];

  constructor() {
    // Sayfa değişince mobil menüyü kapat
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        map(() => false),
        takeUntilDestroyed() // component yok olunca aboneliği otomatik kapat (memory leak olmasın)
      )
      .subscribe((v) => this.menuOpen.set(v));
  }

  // Sayfa biraz kaydırılınca header'a alt çizgi ekleniyor
  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 8);
  }

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  scrollTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
