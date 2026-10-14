import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/api/problem';
import { ToastService } from '../../core/toast.service';
import { PageHeader } from '../shared/page-header';

// İki şifre alanı aynı mı? (form seviyesinde validator)
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const pass = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pass && confirm && pass !== confirm ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings-page',
  imports: [ReactiveFormsModule, PageHeader],
  template: `
    <app-page-header title="Ayarlar" description="Şifre değiştirince açık olan tüm oturumlar kapanır." />

    <form class="card box" [formGroup]="form" (ngSubmit)="submit()" novalidate>
      <h2>Şifre değiştir</h2>

      <div class="field">
        <label for="current">Mevcut şifre</label>
        <input id="current" class="input" type="password" formControlName="currentPassword" autocomplete="current-password" />
      </div>

      <div class="field">
        <label for="new">Yeni şifre</label>
        <input id="new" class="input" type="password" formControlName="newPassword" autocomplete="new-password" />
        <span class="hint">En az 10 karakter, büyük harf, küçük harf ve rakam içermeli.</span>
        @if (form.controls.newPassword.touched && form.controls.newPassword.invalid) {
          <span class="error">Şifre kurallara uymuyor.</span>
        }
      </div>

      <div class="field">
        <label for="confirm">Yeni şifre (tekrar)</label>
        <input id="confirm" class="input" type="password" formControlName="confirmPassword" autocomplete="new-password" />
        @if (form.hasError('mismatch') && form.controls.confirmPassword.touched) {
          <span class="error">Şifreler aynı değil.</span>
        }
      </div>

      <button type="submit" class="btn btn-primary" [disabled]="saving()">
        {{ saving() ? 'Kaydediliyor...' : 'Şifreyi değiştir' }}
      </button>
    </form>
  `,
  styles: `
    .box { max-width: 480px; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    h2 { font-size: 1.05rem; }
    button { align-self: flex-start; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPage {
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  protected readonly saving = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group(
    {
      currentPassword: ['', Validators.required],
      newPassword: [
        '',
        [Validators.required, Validators.minLength(10), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)]
      ],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword } = this.form.getRawValue();
    this.saving.set(true);

    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.toast.success('Şifre değişti. Lütfen yeni şifrenle tekrar giriş yap.');
        this.auth.clearSession();
        this.auth.logout();
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.toast.error(problemMessage(err, 'Şifre değiştirilemedi.'));
      }
    });
  }
}
