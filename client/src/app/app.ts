import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/theme.service';
import { I18nService } from './core/i18n/i18n.service';
import { ToastHost } from './shared/toast-host';
import { ConfirmHost } from './shared/confirm-host';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastHost, ConfirmHost],
  template: `
    <router-outlet />
    <app-toast-host />
    <app-confirm-host />
  `
})
export class App {
  // Servisleri burada inject ediyoruz ki uygulama açılır açılmaz tema ve dil effect'leri çalışsın
  protected readonly theme = inject(ThemeService);
  protected readonly i18n = inject(I18nService);
}
