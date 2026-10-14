import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Profile } from '../../../core/models';
import { PortfolioApi } from '../../../core/api/portfolio-api.service';
import { TranslatePipe } from '../../../core/i18n/i18n.pipes';
import { Icon } from '../../../shared/icon';
import { RevealDirective } from '../../../shared/reveal.directive';

type SendState = 'idle' | 'sending' | 'success' | 'error' | 'rateLimited';

@Component({
  selector: 'app-contact-section',
  imports: [ReactiveFormsModule, TranslatePipe, Icon, RevealDirective],
  templateUrl: './contact-section.html',
  styleUrl: './contact-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactSection {
  private readonly api = inject(PortfolioApi);
  private readonly fb = inject(NonNullableFormBuilder);

  readonly profile = input<Profile | null>(null);
  protected readonly state = signal<SendState>('idle');

  // NOT: Backend'deki FluentValidation kurallarının aynısı burada da var.
  // Frontend doğrulaması kullanıcıya anında geri bildirim için, asıl kontrol yine backend'de.
  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    subject: ['', [Validators.maxLength(150)]],
    message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(4000)]],
    website: [''] // honeypot: gerçek kullanıcı görmüyor, bot dolduruyor
  });

  protected hasError(field: 'name' | 'email' | 'message'): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.touched || control.dirty);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.state.set('sending');

    this.api
      .sendMessage({
        name: value.name.trim(),
        email: value.email.trim(),
        subject: value.subject.trim() || null,
        message: value.message.trim(),
        website: value.website || null
      })
      .subscribe({
        next: () => {
          this.state.set('success');
          this.form.reset();
        },
        error: (err: unknown) => {
          this.state.set(err instanceof HttpErrorResponse && err.status === 429 ? 'rateLimited' : 'error');
        }
      });
  }
}
