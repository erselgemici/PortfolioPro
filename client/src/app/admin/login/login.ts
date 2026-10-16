import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/api/problem';
import { ThemeService } from '../../core/theme.service';
import { Icon } from '../../shared/icon';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, Icon],
  template: `
    <div class="page">
      <button type="button" class="icon-btn theme" (click)="theme.toggle()" aria-label="Temayı değiştir">
        <app-icon [name]="theme.resolved() === 'dark' ? 'sun' : 'moon'" />
      </button>

      <form class="card box" [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <div class="head">
          <span class="logo mono">EG</span>
          <h1>Yönetim paneli</h1>
          <p class="muted">Devam etmek için giriş yap.</p>
        </div>

        @if (error()) {
          <div class="alert" role="alert"><app-icon name="alert" /> {{ error() }}</div>
        }

        <div class="field">
          <label for="email">E-posta</label>
          <input id="email" class="input" type="email" formControlName="email" autocomplete="username" />
        </div>

        <div class="field">
          <label for="password">Şifre</label>
          <input id="password" class="input" type="password" formControlName="password" autocomplete="current-password" />
        </div>

        <button type="submit" class="btn btn-primary" [disabled]="loading()">
          <app-icon name="lock" /> {{ loading() ? 'Giriş yapılıyor...' : 'Giriş yap' }}
        </button>

        <a routerLink="/" class="back"><app-icon name="arrow-left" /> Siteye dön</a>
      </form>
    </div>
  `,
  styles: `
    .page {
      min-height: 100vh; display: grid; place-items: center; padding: 24px;
      background: radial-gradient(600px circle at 50% 0%, var(--accent-soft), transparent 70%);
    }
    .theme { position: fixed; top: 16px; right: 16px; }
    .box { width: min(400px, 100%); padding: 32px; display: flex; flex-direction: column; gap: 18px; box-shadow: var(--shadow); }
    .head { display: flex; flex-direction: column; align-items: flex-start; gap: 6px; margin-bottom: 6px; }
    .logo {
      display: grid; place-items: center; width: 40px; height: 40px; border-radius: 11px; margin-bottom: 10px;
      background: var(--accent); color: var(--accent-contrast); font-weight: 700; font-size: 0.85rem;
    }
    h1 { font-size: 1.4rem; }
    .alert { display: flex; gap: 8px; align-items: flex-start; padding: 10px 12px; border-radius: 10px; background: var(--danger-soft); color: var(--danger); font-size: 0.9rem; }
    .alert app-icon { width: 18px; height: 18px; margin-top: 2px; }
    .back { display: inline-flex; align-items: center; gap: 6px; justify-content: center; color: var(--text-muted); font-size: 0.88rem; }
    .back app-icon { width: 16px; height: 16px; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly theme = inject(ThemeService);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('E-posta ve şifre giriniz.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => {
        // Guard bizi login'e yönlendirdiyse, girmek istediğimiz sayfaya geri dön.
        // NOT: Sadece "/admin" ile başlayan adreslere geri dönüyoruz. Yoksa ?returnUrl=https://kotusite.com
        // gibi bir linkle kullanıcı giriş yaptıktan sonra başka bir siteye gönderilebilirdi.
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(returnUrl?.startsWith('/admin') ? returnUrl : '/admin');
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.error.set(problemMessage(err, 'Giriş yapılamadı.'));
      }
    });
  }
}
